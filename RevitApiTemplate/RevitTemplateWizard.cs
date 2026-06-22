using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace RevitApiTemplate
{
    public sealed class RevitTemplateWizard : IWizard
    {
        private bool _createBundleFiles = true;
        private bool _useObfuscar = true;

        public void RunStarted(
            object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind,
            object[] customParams)
        {
            using (var form = new BundleOptionsForm())
            {
                if (form.ShowDialog() != DialogResult.OK)
                    throw new WizardBackoutException();

                _createBundleFiles = form.CreateBundleFiles;
                _useObfuscar = form.UseObfuscar;
            }

            replacementsDictionary["$createbundlefiles$"] = _createBundleFiles ? "true" : "false";
            replacementsDictionary["$useobfuscar$"] = _useObfuscar ? "true" : "false";
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            var normalized = filePath.Replace('/', '\\');

            if (!_createBundleFiles &&
                (normalized.EndsWith("App.cs", StringComparison.OrdinalIgnoreCase) ||
                 normalized.EndsWith("PackageContents.xml", StringComparison.OrdinalIgnoreCase) ||
                 normalized.EndsWith(".addin", StringComparison.OrdinalIgnoreCase) ||
                 normalized.EndsWith("PostBuildEvents.targets", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            if (!_useObfuscar &&
                normalized.IndexOf("\\obfuscar.", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }

            return true;
        }

        public void ProjectFinishedGenerating(Project project)
        {
            if (_createBundleFiles && _useObfuscar)
                return;

            ThreadHelper.ThrowIfNotOnUIThread();
            var projectPath = project.FullName;
            var projectDirectory = Path.GetDirectoryName(projectPath);
            if (projectDirectory == null)
                return;

            if (!_createBundleFiles)
            {
                DeleteIfExists(Path.Combine(projectDirectory, "App.cs"));
                DeleteIfExists(Path.Combine(projectDirectory, "PackageContents.xml"));
                DeleteMatching(projectDirectory, "*.addin");
                DeleteIfExists(Path.Combine(projectDirectory, "Settings", "PostBuildEvents.targets"));
            }

            if (!_useObfuscar)
                DeleteMatching(Path.Combine(projectDirectory, "Settings"), "obfuscar.*.xml");

            RemoveOptionalBlocksFromProject(projectPath);
        }

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
        }

        private static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        private static void DeleteMatching(string directory, string pattern)
        {
            if (!Directory.Exists(directory))
                return;

            foreach (var file in Directory.GetFiles(directory, pattern))
                File.Delete(file);
        }

        private void RemoveOptionalBlocksFromProject(string projectPath)
        {
            var document = XDocument.Load(projectPath, LoadOptions.PreserveWhitespace);
            var root = document.Root;
            if (root == null)
                return;

            if (!_createBundleFiles)
                RemoveBundleBlocks(root);

            if (!_useObfuscar)
                RemoveObfuscarBlocks(root);

            document.Save(projectPath);
        }

        private static void RemoveBundleBlocks(XElement root)
        {
            RemoveElements(root, "Import", element =>
                AttributeContains(element, "Project", "PostBuildEvents.targets"));

            RemoveElements(root, "Target", element =>
                AttributeEqualsAny(element, "Name", "CopyObfuscatedDllToBundle"));

            RemoveElements(root, "BundleFolder", element => true);

            RemoveElements(root, "Message", element =>
                AttributeContains(element, "Text", "@(BundleFolder)"));

            RemoveElements(root, "RemoveDir", element =>
                AttributeContains(element, "Directories", "@(BundleFolder)"));

            RemoveCommentsContaining(root, "Post Build", "Copy to Bundle after obfuscar");
        }

        private static void RemoveObfuscarBlocks(XElement root)
        {
            RemoveElements(root, "Target", element =>
                AttributeEqualsAny(element, "Name", "RunObfuscar", "CopyObfuscatedDllToBundle"));

            RemoveElements(root, "ObfuscarFolder", element => true);

            RemoveElements(root, "Message", element =>
                AttributeContains(element, "Text", "@(ObfuscarFolder)"));

            RemoveElements(root, "RemoveDir", element =>
                AttributeContains(element, "Directories", "@(ObfuscarFolder)"));

            RemoveElements(root, "PackageReference", element =>
                AttributeEqualsAny(element, "Include", "MSBuild.Obfuscar", "Obfuscar"));

            RemovePropertyGroupsContaining(root, "ObfuscarExe");
            RemoveCommentsContaining(root, "Define obfuscar Exe", "Post evnet for running obfuscar", "Copy to Bundle after obfuscar");
        }

        private static void RemoveElements(XElement root, string localName, Func<XElement, bool> predicate)
        {
            var elements = root
                .Descendants()
                .Where(element => element.Name.LocalName == localName && predicate(element))
                .ToList();

            foreach (var element in elements)
                element.Remove();
        }

        private static void RemovePropertyGroupsContaining(XElement root, string childLocalName)
        {
            var propertyGroups = root
                .Elements()
                .Where(element =>
                    element.Name.LocalName == "PropertyGroup" &&
                    element.Elements().Any(child => child.Name.LocalName == childLocalName))
                .ToList();

            foreach (var propertyGroup in propertyGroups)
                propertyGroup.Remove();
        }

        private static void RemoveCommentsContaining(XElement root, params string[] values)
        {
            var comments = root
                .DescendantNodes()
                .OfType<XComment>()
                .Where(comment => values.Any(value =>
                    comment.Value.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0))
                .ToList();

            foreach (var comment in comments)
                comment.Remove();
        }

        private static bool AttributeContains(XElement element, string attributeName, string value)
        {
            var attributeValue = (string)element.Attribute(attributeName);
            return attributeValue?.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool AttributeEqualsAny(XElement element, string attributeName, params string[] values)
        {
            var attributeValue = (string)element.Attribute(attributeName);
            return values.Any(value => string.Equals(attributeValue, value, StringComparison.OrdinalIgnoreCase));
        }
    }
}
