//-----------------------------------------------------------------------
// <copyright file="AzureOperationDesigner.xaml.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Azure
{
    using System;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Metadata;
    using System.ComponentModel;

    /// <summary>
    /// Designer implementation for custom workflow activity.
    /// </summary>
    public partial class AzureOperationDesigner
    {
        public AzureOperationDesigner()
        {
            this.InitializeComponent();
            this.RegisterMetadata();
        }

        private void RegisterMetadata()
        {
            Type type = typeof(AzureAsyncOperation);
            AttributeTableBuilder builder = new AttributeTableBuilder();

            builder.AddCustomAttributes(type, new Attribute[] { new DesignerAttribute(typeof(AzureOperationDesigner)) });
            builder.AddCustomAttributes(type, new ActivityDesignerOptionsAttribute { AllowDrillIn = false }); 
            builder.AddCustomAttributes(type, type.GetProperty("Operation"), new Attribute[] { BrowsableAttribute.No });
            builder.AddCustomAttributes(type, type.GetProperty("Success"), new Attribute[] { BrowsableAttribute.No });
            builder.AddCustomAttributes(type, type.GetProperty("Failure"), new Attribute[] { BrowsableAttribute.No });

            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}
