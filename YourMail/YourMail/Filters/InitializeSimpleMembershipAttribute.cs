﻿using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Web.Mvc;
using WebMatrix.WebData;
using YourMail.Models;
using System.IO;
using System.Web;

namespace YourMail.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InitializeSimpleMembershipAttribute : ActionFilterAttribute
    {
        private static SimpleMembershipInitializer _initializer;
        private static object _initializerLock = new object();
        private static bool _isInitialized;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            LazyInitializer.EnsureInitialized(ref _initializer, ref _isInitialized, ref _initializerLock);
        }

        private class SimpleMembershipInitializer
        {
            public SimpleMembershipInitializer()
            {
                Database.SetInitializer<DataBaseContext>(null);

                try
                {
                    using (var context = new DataBaseContext())
                    {
                        //context.Database.Delete();
                        var folderFordb = HttpContext.Current.Server.MapPath("~/App_Data");
                        if (!Directory.Exists(folderFordb))
                        {
                            Directory.CreateDirectory(folderFordb);
                        }
                        if (!context.Database.Exists())
                        {     
                            ((IObjectContextAdapter)context).ObjectContext.CreateDatabase();
                        }
                    }
                    WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "Id", "UserMail", autoCreateTables: true);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("The ASP.NET Simple Membership database could not be initialized", ex);
                }
            }
        }
    }
}