using Moq;
using NUnit.Framework;
using Service_Layer.Services;
using ShopDemo.Areas.Admin.Controllers;
using ShopDemo.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ShopDemo.Tests.Areas.Admin.Controllers
{
    [TestFixture]
    public class LogControllerTest
    {
        Mock<ILogFileManagementService> mockLogService;

        [SetUp]
        public void SetUp()
        {
            mockLogService = new Mock<ILogFileManagementService>();
        }

        [Test]
        public void Index_returns_view_with_log_files_names()
        {
            List<string> filesNames = new List<string>();
            filesNames.Add("file1.txt");
            filesNames.Add("file2.txt");
            filesNames.Add("file3.txt");
            filesNames.Add("file4.txt");

            mockLogService.Setup(m => m.GetLogFilesNames()).Returns(filesNames);

            LogController controller = new LogController(mockLogService.Object);

            var result = controller.Index() as ViewResult;
            var files = ((IndexLogViewModel)result.Model).Files.ToArray();

            Assert.IsNotNull(result);
            Assert.AreEqual("file1.txt", files[0].Value);
            Assert.AreEqual("file2.txt", files[1].Value);
            Assert.AreEqual("file3.txt", files[2].Value);
            Assert.AreEqual("file4.txt", files[3].Value);
        }

        #region ReadLogs
        [Test]
        public async Task ReadLogs_returns_string_content_result_if_selectedFile_is_null_or_empty()
        {
            LogController controller = new LogController(mockLogService.Object);

            var result = await controller.ReadLogs();

            Assert.That(result, Is.InstanceOf<ContentResult>());
            Assert.AreEqual("Brak danych", ((ContentResult)result).Content);
        }

        [Test]
        public async Task ReadLogs_returns_string_content_result_if_log_file_json_string_is_null_or_empty()
        {
            mockLogService.Setup(m => m.ReadFileAsync(It.IsAny<string>())).ReturnsAsync(string.Empty);

            LogController controller = new LogController(mockLogService.Object);

            var result = await controller.ReadLogs("testfile.txt");

            Assert.That(result, Is.InstanceOf<ContentResult>());
            Assert.AreEqual("Brak danych", ((ContentResult)result).Content);
        }

        [Test]
        public async Task ReadLogs_returns_partial_view_with_log_file_model_ordered_by_Time()
        {
            string jsonLogString = "[{'time': '2018-02-16 00:52:50.0568','level': 'ERROR','message': 'Exception1'},"
                + "{'time': '2018-02-16 11:42:50.0568','level': 'ERROR','message': 'Exception2'},"
                + "{'time': '2018-02-16 08:21:50.0568','level': 'ERROR','message': 'Exception3'}]";

            mockLogService.Setup(m => m.ReadFileAsync(It.IsAny<string>())).ReturnsAsync(jsonLogString);

            LogController controller = new LogController(mockLogService.Object);

            var result = await controller.ReadLogs("testfile.txt") as PartialViewResult;
            var resultModel = ((IEnumerable<LogFileDetailsViewModel>)result.Model).ToArray();

            Assert.IsNotNull(result);
            Assert.AreEqual("_LogFilePartial", result.ViewName);
            Assert.AreEqual("Exception2", resultModel[0].Message);
            Assert.AreEqual("Exception3", resultModel[1].Message);
            Assert.AreEqual("Exception1", resultModel[2].Message);
            Assert.That((IEnumerable<LogFileDetailsViewModel>)result.Model, Is.Ordered.Descending.By("Time"));
        }
        #endregion

        #region DeleteLogFile
        [Test]
        public void DeleteLogFile_returns_error_message_as_json_result_if_selectedFile_is_null_or_empty()
        {
            LogController controller = new LogController(mockLogService.Object);

            var result = controller.DeleteLogFile() as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public void DeleteLogFile_returns_error_message_as_json_result_if_log_file_not_exists()
        {
            mockLogService.Setup(m => m.FileExists(It.IsAny<string>())).Returns(false);

            LogController controller = new LogController(mockLogService.Object);

            var result = controller.DeleteLogFile("testFile.txt") as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsFalse(jsonData.success);
            Assert.IsNotNull(jsonData.message);
        }

        [Test]
        public void DeleteLogFile_removes_log_file()
        {
            mockLogService.Setup(m => m.FileExists(It.IsAny<string>())).Returns(true);

            LogController controller = new LogController(mockLogService.Object);

            var result = controller.DeleteLogFile("testFile.txt") as JsonResult;
            dynamic jsonData = result.Data;

            Assert.IsNotNull(result);
            Assert.IsTrue(jsonData.success);
            mockLogService.Verify(m => m.DeleteFile("testFile.txt"), Times.Once);
        }
        #endregion
    }
}