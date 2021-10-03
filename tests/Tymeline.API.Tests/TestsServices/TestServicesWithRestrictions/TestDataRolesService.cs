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
    public class TestDataRolesService : OneTimeSetUpAttribute
    {
        ITymelineService _timelineService;
        IDataRolesService _dataRolesService;
        List<TymelineObject> tymelineList;
        IDataRolesDao _dataRolesDao;
        public TestState state;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Moq.Mock<IDataRolesDao> _dataRolesDao = new Mock<IDataRolesDao>();
            Moq.Mock<ITymelineObjectDao> _timelineObjectDao = new Moq.Mock<ITymelineObjectDao>();
            _timelineService = new TymelineService(_timelineObjectDao.Object);
            _dataRolesService = new DataRolesService(_dataRolesDao.Object);

            state = new TestState();
        }

        // [Test]
        // public void Test_UserHasAccessToItem_()
        // {
        //     _dataRolesService.UserHasAccessToItem()
        // }










    }
}