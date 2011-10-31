//-----------------------------------------------------------------------
// <copyright file="WorkflowRawPsHostUi.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildExtensions.Activities.Scripting
{
    using System;
    using System.Management.Automation.Host;

    internal class WorkflowRawPsHostUi : PSHostRawUserInterface
    {
        public override ConsoleColor BackgroundColor
        {
            get { return ConsoleColor.White; }
            set { }
        }

        public override Size BufferSize
        {
            get { return new Size(1024, 768); }
            set { }
        }

        public override Coordinates CursorPosition
        {
            get { return new Coordinates(); }
            set { }
        }

        public override int CursorSize
        {
            get { return 1; }
            set { }
        }

        public override ConsoleColor ForegroundColor
        {
            get { return ConsoleColor.Black; }
            set { }
        }

        public override bool KeyAvailable
        {
            get { return true; }
        }

        public override Size MaxPhysicalWindowSize
        {
            get { return new Size(1024, 768); }
        }

        public override Size MaxWindowSize
        {
            get { return new Size(1024, 768); }
        }

        public override Coordinates WindowPosition
        {
            get { return new Coordinates(); }
            set { }
        }

        public override Size WindowSize
        {
            get { return new Size(1024, 768); }
            set { }
        }

        public override string WindowTitle
        {
            get { return "Build Log"; }
            set { }
        }

        public override void FlushInputBuffer()
        {
            return;
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            return null;
        }

        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            return new KeyInfo();
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
        }
    }
}
