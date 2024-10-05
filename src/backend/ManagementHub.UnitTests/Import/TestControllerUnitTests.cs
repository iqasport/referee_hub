using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagementHub.Models.Abstraction.Commands.Import;
using ManagementHub.Models.Domain.Tests;
using ManagementHub.Service.Areas.Tests;
using ManagementHub.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ManagementHub.UnitTests.Import;
public class TestControllerUnitTests
{
	// The point of this test is to assert that CSV files in different formats can be imported
	public static IEnumerable<object[]> TranslationCSV => new List<object[]>
	{
		new object[]
		{
			"""
			sequenceNum,Question Description,Points Available,Correct Answer,Answer 1,Answer 2,Answer 3,Answer 4
			2,question1,1,Answer 2,answer1,answer2,answer3,answer4
			3,question2,1,Answer 3,answer1,answer2,answer3,answer4
			"""
		},
		new object[]
		{
			"""
			sequenceNum,Question,Feedback,Correct,Answer1,Answer2,Answer3,Answer4
			2,question1,,2,answer1,answer2,answer3,answer4
			3,question2,,3,answer1,answer2,answer3,answer4
			"""
		}
	};

	[Theory]
	[MemberData(nameof(TranslationCSV))]
	public async Task GivenTranslationCSV_WhenImported_ThenTranslationsAreParsed(string csv)
	{
		// Arrange
		var dbContext = new ManagementHubDbContext();
		var importTestQuestions = new Mock<IImportTestQuestions>();
		
		var imported = new List<TestQuestionRecord>();
		importTestQuestions.Setup(i => i.ImportTestQuestionsAsync(It.IsAny<TestIdentifier>(), It.IsAny<IEnumerable<TestQuestionRecord>>()))
			.Callback<TestIdentifier, IEnumerable<TestQuestionRecord>>((testId, questions) => imported.AddRange(questions));

		var testId = new TestIdentifier();
		var controller = new TestsController(importTestQuestions.Object, dbContext);

		controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext()
			{
				Request =
				{
					Body = new MemoryStream(Encoding.UTF8.GetBytes(csv)),
				}
			}
		};

		// Act & Assert
		await controller.ImportTestQuestions(testId);

		// Assert
		Assert.Equal(2, imported.Count);
		
		Assert.Equal(2, imported[0].SequenceNum);
		Assert.Equal("question1", imported[0].Question);
		Assert.Equal(2, imported[0].Correct);
		Assert.Equal("answer1", imported[0].Answer1);
		Assert.Equal("answer2", imported[0].Answer2);
		Assert.Equal("answer3", imported[0].Answer3);
		Assert.Equal("answer4", imported[0].Answer4);
		
		Assert.Equal(3, imported[1].SequenceNum);
		Assert.Equal("question2", imported[1].Question);
		Assert.Equal(3, imported[1].Correct);
		Assert.Equal("answer1", imported[1].Answer1);
		Assert.Equal("answer2", imported[1].Answer2);
		Assert.Equal("answer3", imported[1].Answer3);
		Assert.Equal("answer4", imported[1].Answer4);
	}
}
