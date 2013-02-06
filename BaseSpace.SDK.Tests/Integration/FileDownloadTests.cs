﻿using Illumina.BaseSpace.SDK.ServiceModels;
using Illumina.BaseSpace.SDK.Tests.Helpers;
using Illumina.BaseSpace.SDK.Types;
using Xunit;
using File = System.IO.File;
using System.IO;

namespace Illumina.BaseSpace.SDK.Tests.Integration
{
    public class FileDownloadTests : BaseIntegrationTest
    {
        [Fact]
        public void CanUploadSmallFileToAppResult_SinglePart()
        {
            var project = TestHelpers.CreateRandomTestProject(Client);
            var appResult = TestHelpers.CreateRandomTestAppResult(Client, project);
            var file = File.Create(string.Format("UnitTestFile_{0}", appResult.Id));
            file.Write(new byte[10], 0, 10);
            file.Close();
            var response = Client.UploadFileToAppResult(
                new UploadFileToAppResultRequest(appResult.Id, file.Name, false), null);
            Assert.NotNull(response);
            Assert.True(response.UploadStatus == FileUploadStatus.complete);

            var res2 = Client.GetFileContentUrl(new FileContentRedirectMetaRequest(response.Id));
            var a = res2;

            //var fs = new FileStream("DownloadedFile-" + StringHelpers.RandomAlphanumericString(5), FileMode.OpenOrCreate);
            //Client.DownloadFileTaskByIdAsync(response.Id, fs).Wait();
        }
    }
}
