﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BuggyDemoCode.Services;
using System.Threading;

namespace BuggyDemoWeb.Controllers
{
    public class UnresponsiveLowCPUController : BaseController
    {
        private readonly LegacyService legacyService;

        public UnresponsiveLowCPUController(LegacyService legacyService)
        {
            this.legacyService = legacyService;
        }

        public IActionResult Index()
        {
            return Ok();
        }

        /// <summary>
        /// sudo apt-get install siege
        /// e.g. siege -c 100 -t1M https://localhost:5001/lowcpu/uses-too-many-threadpool-threads-v1
        /// </summary>
        /// <returns></returns>
        [HttpGet("lowcpu/uses-too-many-threadpool-threads-v1")]
        public IActionResult SyncOverAsyncResultV1()
        {
            string val = legacyService.DoAsyncOperationWell().Result;

            return Ok(val);
        }

        /// <summary>
        /// sudo apt-get install siege
        /// e.g. siege -c 100 -t1M https://localhost:5001/lowcpu/uses-too-many-threadpool-threads-v2
        /// </summary>
        /// <returns></returns>
        [HttpGet("lowcpu/uses-too-many-threadpool-threads-v2")]
        public IActionResult SyncOverAsyncResultV2()
        {
            string val = Task.Run(() => legacyService.DoAsyncOperationWell()).GetAwaiter().GetResult();

            return Ok(val);
        }

        /// <summary>
        /// sudo apt-get install siege
        /// e.g. siege -c 100 -t1M https://localhost:5001/lowcpu/uses-too-many-threadpool-threads-v3
        /// </summary>
        /// <returns></returns>
        [HttpGet("lowcpu/uses-too-many-threadpool-threads-v3")]
        public IActionResult SyncOverAsyncResultV3()
        {
            string val = Task.Run(() => legacyService.DoAsyncOperationWell().GetAwaiter().GetResult()).GetAwaiter().GetResult();

            return Ok(val);
        }

        /// <summary>
        /// sudo apt-get install siege
        /// e.g. siege -c 100 -t1M https://localhost:5001/lowcpu/uses-too-many-threadpool-threads-v4
        /// </summary>
        /// <returns></returns>
        [HttpGet("lowcpu/uses-too-many-threadpool-threads-v4")]
        public IActionResult SyncOverAsyncResultV4()
        {

            var task = legacyService.DoAsyncOperationWell();
            task.Wait();
            string val = task.GetAwaiter().GetResult();

            return Ok(val);
        }


        /// <summary>
        /// sudo apt-get install siege
        /// siege -c 100 -t1M https://localhost:5001/lowcpu/uses-too-many-threadpool-threads-fixed
        /// </summary>
        /// <returns></returns>
        [HttpGet("lowcpu/uses-too-many-threadpool-threads-fixed")]
        public async Task<IActionResult> SyncOverAsyncResultFixed()
        {
            var result = await legacyService.DoAsyncOperationWell();

            return Ok(result);
        }

        /// <summary>
        /// Deadlock...
        /// </summary>
        /// <returns></returns>
        [HttpGet("lowcpu/deadlocked-tasks-v1")]
        public async Task<IActionResult> CreateDeadlockTask()
        {
            await legacyService.SimpleDeadLockAsyncOperation();

            return Ok();
        }

        /// <summary>
        /// Deadlock with a semaphore
        /// </summary>
        /// <returns></returns>
        [HttpGet("lowcpu/deadlocked-tasks-v2")]
        public async Task<IActionResult> CreateDeadlockTaskSemaphore()
        {
            await legacyService.SemaphoreDeadLockAsyncOperations();

            return Ok();
        }

        /// <summary>
        /// Thread blocked on a lock owned by another thread...
        /// </summary>
        /// <returns></returns>
        [HttpGet("lowcpu/deadlocked-tasks-v3")]
        public IActionResult CreateDeadlockTaskYieldReturn()
        {
            legacyService.ReallyBadYieldReturn().Wait();

            return Ok();
        }
    }
}
