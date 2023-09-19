using System;
using System.Threading.Tasks;
using System.Linq;
using AuthenticationManager.Data.Context;
using AuthenticationManager.Data.Models;
using System.Collections.Generic;
using AuthenticationManager.Data.Repositories;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NUnit.Framework;

namespace AuthenticationManager.Tests.UnitTests
{
    public class BaseTest
    {
        protected readonly IAuthManagerRepository _repository;

        public BaseTest()
        {
            _repository = Substitute.For<IAuthManagerRepository>();
        }
    }
}
