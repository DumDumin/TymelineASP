using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Tymeline.API.Controllers;

namespace Tymeline.API.Tests
{
    public class TymelineServiceTest : OneTimeSetUpAttribute
    {
        ITymelineService _timelineService;
        List<TymelineObject> tymelineList;


        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Moq.Mock<ITymelineObjectDao> _timelineObjectDao = new Moq.Mock<ITymelineObjectDao>();
            _timelineService = new TymelineService(_timelineObjectDao.Object);
            
            _timelineObjectDao.Setup(s => s.getAll()).Returns(() => mockGetAll());
            _timelineObjectDao.Setup(x => x.getById(It.IsAny<string>())).Returns((string id) => mockGetById(id));
            _timelineObjectDao.Setup(x => x.getByTime(It.IsAny<int>(),It.IsAny<int>())).Returns((int start,int end) => mockDaoGetByTime(start,end));
            _timelineObjectDao.Setup(x => x.DeleteById(It.IsAny<string>())).Callback((string id) => mockDeleteById(id));
            _timelineObjectDao.Setup(x => x.Create(It.IsAny<TymelineObject>())).Returns((TymelineObject to) => mockCreate(to));
            _timelineObjectDao.Setup(x => x.UpdateById(It.IsAny<string>(), It.IsAny<TymelineObject>())).Returns((string id,TymelineObject tO) => mockUpdateById(id,tO));
        }
        

        [SetUp]
        public void SetUp(){
            tymelineList = setupTymelineList();
        }


        TymelineObject mockUpdateById(string id, TymelineObject tymelineObject){
            mockDeleteById(id);
            return mockCreate(tymelineObject);
        }

        TymelineObject mockCreate(TymelineObject to){
                to.Id = new Guid().ToString();
                tymelineList.Add(to);
                return to;
            
        }

        List<TymelineObject> mockGetAll(){
            return tymelineList;
        }

        [TearDown]
        public void TearDown(){
            tymelineList.Clear();
        }

        private TymelineObject mockGetById(string id){
            var s = tymelineList.Find(obj => obj.Id.Equals(id));
            if (s == null){
                throw new KeyNotFoundException();
            }
            return s;
        }

         private void mockDeleteById(string id){
            var element = tymelineList.Find(element => element.Id.Equals(id));
            tymelineList.Remove(element);
        }
        List<TymelineObject> mockDaoGetByTime(int start,int end){
            var s = tymelineList.Where(element => start<element.Start+element.Length && start>element.Start+element.Length).ToList();
            s.AddRange(tymelineList.Where(element => start<element.Start && element.Start<end).ToList());
            return s.Distinct().ToList();
        }

         private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static bool RandomBool()
        {
            return (random.Next() % 2)==1;
        }

       


        static private List<TymelineObject> setupTymelineList(){
            List<TymelineObject> array = new List<TymelineObject>();
            for (int i = 1; i < 100; i++)
            {

                array.Add( new TymelineObject() {
                    Id=i.ToString(),
                    Length=500+(random.Next() % 5000),
                    Content=new Content(RandomString(12)),
                    Start=10000+(random.Next() % 5000),
                    CanChangeLength=RandomBool(),
                    CanMove=RandomBool()
                    }
                );
            }
            return array;
        }

        [Test]
        public void TestGetAll()
        {   
            
            Assert.IsInstanceOf<List<TymelineObject>>(_timelineService.GetAll());
        }

        [Test]
        public void TestGetById_With_Existing_Element_Expect_Element(){
            TymelineObject tymelineObject =  _timelineService.GetById("1");
            Assert.IsInstanceOf<TymelineObject>(tymelineObject);
            Assert.AreEqual(tymelineList[0],tymelineObject);

        }


        [Test]
        public void TestGetById_With_Not_Existing_Element_Expect_Exceptions(){
            Assert.Throws<KeyNotFoundException>(()=> _timelineService.GetById("2001"));
        }


        [Test]
        public void TestGetById_With_Existing_Element_Expect_Not_null_Return(){
            Assert.NotNull(_timelineService.GetById("2"));
        }

        [Test]
        public void TestGetByTime_With_Valid_Time_Expect_List_TymelineObjects(){
            Assert.IsInstanceOf<List<TymelineObject>>(_timelineService.GetByTime(10000,15000));
        }

        [Test]
        public void Test_DeleteById_Expect_Item_to_be_Deleted(){
            _timelineService.DeleteById("2");
            Assert.Throws<KeyNotFoundException>(()=> _timelineService.GetById("2"));
        }


        [Test]

        public void Test_Create_Element_Expect_Element_to_be_Returned(){
            var element = new TymelineObject{CanChangeLength=true,CanMove=true,Content=new Content("asd"),Id="asd",Length=12389,Start=12379};
            _timelineService.Create(element);
            Assert.AreEqual(element,_timelineService.GetById(element.Id));
        }

        [Test]
        public void Test_Create_Existing_Element_Expect_UpdatedElement_To_Differ_From_Existing_Element(){
            var element = new TymelineObject{CanChangeLength=true,CanMove=true,Content=new Content("asd"),Id="5",Length=12389,Start=12379};
            _timelineService.Create(element);
            Assert.IsFalse(_timelineService.GetById("5").Same(element));
        }


        [Test]
        public void Test_Create_Existing_Element_Expect_NewElement_To_Be_Itself(){
            var element = new TymelineObject{CanChangeLength=true,CanMove=true,Content=new Content("asd"),Id="5",Length=12389,Start=12379};
            var newElement = _timelineService.Create(element);
            Assert.IsTrue(_timelineService.GetById(newElement.Id).Same(newElement));
        }
        


        public void Test_Update_ExistingObject(){
            var element = new TymelineObject{CanChangeLength=true,CanMove=true,Content=new Content("asd"),Id="5",Length=12389,Start=12379};
            _timelineService.UpdateById("5",element);
            Assert.IsTrue(element.Same(_timelineService.GetById("5")));
        }

        public void Test_Update_ExistingObject_with_non_matching_object_expect_error(){
            var element = new TymelineObject{CanChangeLength=true,CanMove=true,Content=new Content("asd"),Id="105",Length=12389,Start=12379};
            Assert.Throws<ArgumentException>(()=> _timelineService.UpdateById("5",element));
        }







    }
}