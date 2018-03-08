using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using TestDataBuilderGenerator.Settings;

namespace TestDataBuilderGenerator.Helper {
    internal static class ProjectHelper {
        internal static Project GetAccordingTestProject(this INamedTypeSymbol namedTypeSymbol) {
            Project project;
            if (TryGetTestProject(namedTypeSymbol, out project)) {
                return project;
            } else {
                return null;
            }
        }

        internal static ITypeSymbol GetBuilderFactorySymbol(this Project testProject, BuilderCopSettings builderCopSettings) {
            ITypeSymbol createClassTypeSymbol;
            if (testProject.TryGetCreateClassSymbol(builderCopSettings, out createClassTypeSymbol)) {
                return createClassTypeSymbol;
            } else {
                return null;
            }
        }

        internal static bool TryGetCreateClassSymbol(
            this Project testProject, BuilderCopSettings builderCopSettings, out ITypeSymbol createClassSymbol) {
            createClassSymbol = SymbolFinder.FindDeclarationsAsync(testProject, builderCopSettings.BuilderFactorySettings.Name, false, SymbolFilter.Type)
                .Result.FirstOrDefault() as ITypeSymbol;
            return createClassSymbol != null;
        }

        internal static bool TryGetTestProject(this INamedTypeSymbol namedTypeSymbol, out Project testProject) {
            Workspace workspace;
            if (!Workspace.TryGetWorkspace(namedTypeSymbol.Locations.First().SourceTree.GetText().Container, out workspace)) {
                testProject = null;
                return false;
            }

            testProject = workspace.CurrentSolution.Projects.SingleOrDefault(
                project => project.Name == namedTypeSymbol.ContainingAssembly.Name + ".Test");
            return testProject != null;
        }
    }
}