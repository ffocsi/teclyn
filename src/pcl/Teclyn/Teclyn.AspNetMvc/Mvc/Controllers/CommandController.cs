﻿using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Teclyn.AspNetMvc.Mvc.Models;
using Teclyn.AspNetMvc.Mvc.Security;
using Teclyn.Core;
using Teclyn.Core.AutoAnalysis;
using Teclyn.Core.Commands;
using Teclyn.Core.Errors.Models;
using Teclyn.Core.Events;
using Teclyn.Core.Ioc;
using Teclyn.Core.Metadata;
using Teclyn.Core.Security.Context;
using Teclyn.Core.Storage;

namespace Teclyn.AspNetMvc.Mvc.Controllers
{
    public class CommandController : Controller
    {
        [Inject]
        public CommandService CommandService { get; set; }

        [Inject]
        public MetadataRepository MetadataRepository { get; set; }

        [Inject]
        public RepositoryService RepositoryService { get; set; }

        [Inject]
        public IRepository<IError> ErrorRepository { get; set; }

        [Inject]
        public IRepository<IEventInformation> EventInformationRepository { get; set; }

        [Inject]
        public ITeclynContext Context { get; set; }

        [Inject]
        public AutoAnalyzer AutoAnalyzer { get; set; }
        
        [HttpPost]
        public async Task<ActionResult> Execute(IBaseCommand command)
        {
            var result = await this.CommandService.ExecuteGeneric(command);

            return this.Structured(result);
        }
        
        [HttpPost]
        public async Task<ActionResult> ExecutePost(IBaseCommand command, string returnUrl)
        {
            await this.CommandService.ExecuteGeneric(command);
            
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = this.Request.UrlReferrer?.ToString();
            }

            return Redirect(returnUrl);
        }
        
        //[OnlyAdminFilter]
        public ActionResult Info()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            var model = new HomeInfoModel
            {
                Aggregates = this.RepositoryService.Aggregates.Select(agg => new AggregateInfoModel
                {
                    AggregateType = agg.AggregateType.ToString(),
                    ImplementationType = agg.ImplementationType.ToString(),
                }).ToArray(),
                TeclynVersion = version,
                Commands = this.MetadataRepository.Commands,
                Events = this.MetadataRepository.Events,
            };

            return this.Structured(model);
        }

        public ActionResult Info2()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            var model = new HomeInfoModel
            {
                Aggregates = this.RepositoryService.Aggregates.Select(agg => new AggregateInfoModel
                {
                    AggregateType = agg.AggregateType.ToString(),
                    ImplementationType = agg.ImplementationType.ToString(),
                }).ToArray(),
                TeclynVersion = version,
                Commands = this.MetadataRepository.Commands,
                Events = this.MetadataRepository.Events,
            };

            return this.View(model);
        }

        [OnlyAdminFilter]
        public ActionResult Errors()
        {
            var errors = this.ErrorRepository
                .OrderByDescending(error => error.Date)
                .Take(10)
                .ToList();

            return this.Structured(errors);
        }

        [OnlyAdminFilter]
        public ActionResult Events()
        {
            var events = this.EventInformationRepository
                .OrderByDescending(e => e.Date)
                .Take(100)
                .ToList();

            return this.Structured(events);
        }

        //[OnlyAdminFilter]
        public ActionResult Analyze()
        {
            var report = this.AutoAnalyzer.Analyze();

            return this.Structured(report);
        }
    }
}