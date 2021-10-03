using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Tymeline.API.Controllers;

namespace Tymeline.API.Tests
{
    [Category("Service")]
    public class TymelineServiceTest : OneTimeSetUpAttribute
    {
        ITymelineService _timelineService;
        List<TymelineObject> tymelineList;


        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Moq.Mock<ITymelineObjectDao> _timelineObjectDao = new Moq.Mock<ITymelineObjectDao>();
            _timelineService = new TymelineService(_timelineObjectDao.Object);

            tymelineList = TestUtil.setupTymelineList();

            _timelineObjectDao.Setup(s => s.getAll()).Returns(() => mockGetAll());
            _timelineObjectDao.Setup(x => x.getById(It.IsAny<string>())).Returns((string id) => mockGetById(id));
            _timelineObjectDao.Setup(x => x.getByTime(It.IsAny<int>(), It.IsAny<int>())).Returns((int start, int end) => mockDaoGetByTime(start, end));
            _timelineObjectDao.Setup(x => x.DeleteById(It.IsAny<string>())).Callback((string id) => mockDeleteById(id));
            _timelineObjectDao.Setup(x => x.Create(It.IsAny<TymelineObject>())).Returns((TymelineObject to) => mockCreate(to));
            _timelineObjectDao.Setup(x => x.UpdateById(It.IsAny<string>(), It.IsAny<TymelineObject>())).Returns((string id, TymelineObject tO) => mockUpdateById(id, tO));
        }




        TymelineObject mockCreate(TymelineObject to)
        {

            if (tymelineList.Exists(x => x.Id.Equals(to.Id)))
            {
                throw new ArgumentException("you cannot create a TymelineObject with an existing id!");
            }
            else
            {
                if (to.Id == null)
                {
                    to.Id = Guid.NewGuid().ToString();
                }
                tymelineList.Add(to);
                return to;
            }

        }

        List<TymelineObject> mockGetAll()
        {
            return tymelineList;
        }


        private TymelineObject mockGetById(string id)
        {
            try
            {
                return tymelineList.First(obj => obj.Id.Equals(id));

            }
            catch (System.Exception)
            {

                throw new ArgumentException();
            }
        }


        TymelineObject mockUpdateById(string id, TymelineObject tymelineObject)
        {
            mockDeleteById(id);
            return mockCreate(tymelineObject);
        }
        private void mockDeleteById(string id)
        {
            var element = tymelineList.Find(element => element.Id.Equals(id));
            if (element != null)
            {
                //only delete the old one if it exists!
                tymelineList.Remove(element);
            }
        }


        List<TymelineObject> mockDaoGetByTime(int start, int end)
        {
            var s = tymelineList.Where(element => start < element.Start + element.Length && start > element.Start + element.Length).ToList();
            s.AddRange(tymelineList.Where(element => start < element.Start && element.Start < end).ToList());
            return s.Distinct().ToList();
        }




        [Test, AutoData]
        public void TestGetById_With_Not_Existing_Element_Expect_Exceptions(string key)
        {
            Assert.Throws<ArgumentException>(() => _timelineService.GetById(key));
        }


        [Test]
        public void TestGetById_With_Existing_Element_Expect_Not_null_Return()
        {
            var randomElement = _timelineService.GetAllForUser("1", Roles.user).RandomElement();
            _timelineService.GetById(randomElement.Id).Should().Be(randomElement);
        }

        [Test]
        public void TestGetByTime_With_Valid_Time_Expect_List_TymelineObjects()
        {
            Assert.IsInstanceOf<List<TymelineObject>>(_timelineService.GetByTime(10000, 15000));
        }

        [Test]
        public void Test_DeleteById_Expect_Item_to_be_Deleted()
        {
            _timelineService.DeleteById("2");
            Assert.Throws<ArgumentException>(() => _timelineService.GetById("2"));
        }


        [Test]
        public void Test_Create_Element_Expect_Element_to_be_Returned()
        {
            var element = new TymelineObject { Id = Guid.NewGuid().ToString(), CanChangeLength = true, CanMove = true, Content = new Content("asd"), Length = 12389, Start = 12379 };
            _timelineService.Create(element);
            Assert.AreEqual(element, _timelineService.GetById(element.Id));
        }

        [Test]
        public void Test_Create_Element_Without_Id_Expect_Element_to_be_Returned()
        {
            var element = new TymelineObject { CanChangeLength = true, CanMove = true, Content = new Content("asd"), Length = 12389, Start = 12379 };
            _timelineService.Create(element);
            _timelineService.GetById(element.Id).Should().BeEquivalentTo(element, options => options.Excluding(o => o.Id));
        }

        [Test]
        public void Test_Create_Existing_Element_Expect_ArgumentException()
        {
            var truth = tymelineList.RandomElement();
            var element = new TymelineObject { CanChangeLength = true, CanMove = true, Content = new Content("asd"), Id = truth.Id, Length = 12389, Start = 12379 };
            Assert.Throws<ArgumentException>(() => _timelineService.Create(element));


        }


        [Test]
        public void Test_Create_Existing_Element_Expect_NewElement_To_Be_Itself()
        {
            var element = new TymelineObject { Id = Guid.NewGuid().ToString(), CanChangeLength = true, CanMove = true, Content = new Content("asd"), Length = 12389, Start = 12379 };
            var newElement = _timelineService.Create(element);
            Assert.IsTrue(_timelineService.GetById(newElement.Id).Same(newElement));
        }


        [Test]
        public void Test_Update_ExistingObject_Expect_Element_ToUpdate()
        {
            var element = new TymelineObject { CanChangeLength = true, CanMove = true, Content = new Content("asd"), Id = "5", Length = 12389, Start = 12379 };
            _timelineService.UpdateById("5", element);
            Assert.IsTrue(element.Same(_timelineService.GetById("5")));
        }

        [Test]
        public void Test_Update_NewObject_Expect_New_Element_to_be_Created()
        {
            var element = new TymelineObject { CanChangeLength = true, CanMove = true, Content = new Content("asd"), Id = "5123", Length = 12389, Start = 12379 };
            _timelineService.UpdateById("5123", element);
            Assert.IsTrue(element.Same(_timelineService.GetById("5123")));
        }
        [Test]
        public void Test_Update_ExistingObject_with_non_matching_object_expect_ArgumentException()
        {
            var element = new TymelineObject { CanChangeLength = true, CanMove = true, Content = new Content("asd"), Id = "105", Length = 12389, Start = 12379 };
            Assert.Throws<ArgumentException>(() => _timelineService.UpdateById("5", element));
        }







    }
}