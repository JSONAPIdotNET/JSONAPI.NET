using FluentAssertions;
using JSONAPI.Documents.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.Tests.Documents.Builders
{
    [TestClass]
    public class RegistryDrivenDocumentBuilderTests
    {
        [TestMethod]
        public void PathExpressionMatchesCurrentPath_is_true_when_pathToInclude_equals_currentPath_with_one_segment()
        {
            // Arrange
            const string currentPath = "posts";
            const string pathToInclude = "posts";

            // Act
            var matches = RegistryDrivenDocumentBuilder.PathExpressionMatchesCurrentPath(currentPath, pathToInclude);

            // Assert
            matches.Should().BeTrue();
        }

        [TestMethod]
        public void PathExpressionMatchesCurrentPath_is_false_when_pathToInclude_does_not_equal_or_start_with_currentPath()
        {
            // Arrange
            const string currentPath = "posts";
            const string pathToInclude = "comments";

            // Act
            var matches = RegistryDrivenDocumentBuilder.PathExpressionMatchesCurrentPath(currentPath, pathToInclude);

            // Assert
            matches.Should().BeFalse();
        }

        [TestMethod]
        public void PathExpressionMatchesCurrentPath_is_false_when_pathToInclude_is_empty()
        {
            // Arrange
            const string currentPath = "";
            const string pathToInclude = "";

            // Act
            var matches = RegistryDrivenDocumentBuilder.PathExpressionMatchesCurrentPath(currentPath, pathToInclude);

            // Assert
            matches.Should().BeFalse();
        }

        [TestMethod]
        public void PathExpressionMatchesCurrentPath_is_false_when_pathToInclude_is_null()
        {
            // Arrange
            const string currentPath = null;
            const string pathToInclude = null;

            // Act
            var matches = RegistryDrivenDocumentBuilder.PathExpressionMatchesCurrentPath(currentPath, pathToInclude);

            // Assert
            matches.Should().BeFalse();
        }

        [TestMethod]
        public void PathExpressionMatchesCurrentPath_is_true_when_pathToInclude_equals_currentPath_with_multiple_segments()
        {
            // Arrange
            const string currentPath = "posts.author";
            const string pathToInclude = "posts.author";

            // Act
            var matches = RegistryDrivenDocumentBuilder.PathExpressionMatchesCurrentPath(currentPath, pathToInclude);

            // Assert
            matches.Should().BeTrue();
        }

        [TestMethod]
        public void PathExpressionMatchesCurrentPath_is_true_when_all_segments_of_currentPath_are_contained_by_pathToInclude()
        {
            // Arrange
            const string currentPath = "posts.author";
            const string pathToInclude = "posts.author.comments";

            // Act
            var matches = RegistryDrivenDocumentBuilder.PathExpressionMatchesCurrentPath(currentPath, pathToInclude);

            // Assert
            matches.Should().BeTrue();
        }

        [TestMethod]
        public void PathExpressionMatchesCurrentPath_is_false_when_all_segments_of_currentPath_are_contained_by_pathToInclude_but_start_doesnt_match()
        {
            // Arrange
            const string currentPath = "posts.author";
            const string pathToInclude = "author.posts.author";

            // Act
            var matches = RegistryDrivenDocumentBuilder.PathExpressionMatchesCurrentPath(currentPath, pathToInclude);

            // Assert
            matches.Should().BeFalse();
        }

        [TestMethod]
        public void PathExpressionMatchesCurrentPath_is_false_when_pathToInclude_starts_with_currentPath_but_segments_differ()
        {
            // Arrange
            const string currentPath = "posts.author";
            const string pathToInclude = "posts.authora";

            // Act
            var matches = RegistryDrivenDocumentBuilder.PathExpressionMatchesCurrentPath(currentPath, pathToInclude);

            // Assert
            matches.Should().BeFalse();
        }
    }
}
