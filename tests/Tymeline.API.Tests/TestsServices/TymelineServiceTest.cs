using System;
using System.Collections.Generic;
using System.Data.Common;
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
                TymelineObject mockTymelineObject;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Moq.Mock<ITymelineObjectDao> _timelineObjectDao = new Moq.Mock<ITymelineObjectDao>();
            _timelineService = new TymelineService(_timelineObjectDao.Object);
            mockTymelineObject = new TymelineObject("1",10000,new Content("testcontent"),100000000,false,true);
            List<TymelineObject> TymelineObjectList = new List<TymelineObject>();
            TymelineObjectList.Add(mockTymelineObject);
            _timelineObjectDao.Setup(s => s.getAll()).Returns(TymelineObjectList);
            _timelineObjectDao.Setup(x => x.getById(It.IsAny<string>())).Returns((string id) => {
                var obj = TymelineObjectList.Find(obj => obj.Id.Equals(id));
                if (obj!=null){
                    return obj;
                }
                throw new ArgumentException();
                });
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
            Assert.AreEqual(mockTymelineObject,tymelineObject);

        }


        [Test]
        public void TestGetById_With_Not_Existing_Element_Expect_null(){
            Assert.Throws<ArgumentException>(()=> _timelineService.GetById("2"));
           
        }

        [Test]
        public void Test(){
            // _timelineService.Create();
        }






    }
}