﻿// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using ICSharpCode.PythonBinding;
using ICSharpCode.Scripting.Tests.Utils;
using NUnit.Framework;
using PythonBinding.Tests.Utils;

namespace PythonBinding.Tests.Designer
{
	[TestFixture]
	public class LoadSimpleFormTestFixture : LoadFormTestFixtureBase
	{		
		public override string PythonCode {
			get {
				return "class MainForm(System.Windows.Forms.Form):\r\n" +
							"    def InitializeComponent(self):\r\n" +
							"        self.SuspendLayout()\r\n" +
							"        # \r\n" +
							"        # MainForm\r\n" +
							"        # \r\n" +
							"        self.ClientSize = System.Drawing.Size(300, 400)\r\n" +
							"        self.Name = \"MainForm\"\r\n" +
							"        self.ResumeLayout(False)\r\n";
			}
		}
				
		public CreatedComponent FormComponent {
			get { return ComponentCreator.CreatedComponents[0]; }
		}
		
		[Test]
		public void MainFormCreated()
		{			
			Assert.IsNotNull(Form);
		}
		
		[Test]
		public void MainFormName()
		{
			Assert.AreEqual("MainForm", Form.Name);
		}
		
		[Test]
		public void OneComponentCreated()
		{
			Assert.AreEqual(1, ComponentCreator.CreatedComponents.Count);
		}
		
		[Test]
		public void ComponentName()
		{
			Assert.AreEqual("MainForm", FormComponent.Name);
		}
		
		[Test]
		public void ComponentType()
		{
			Assert.AreEqual("System.Windows.Forms.Form", FormComponent.TypeName);
		}
		
		[Test]
		public void FormClientSize()
		{
			Size size = new Size(300, 400);
			Assert.AreEqual(size, Form.ClientSize);
		}

		[Test]
		public void BaseClassTypeNameLookedUp()
		{
			Assert.AreEqual("System.Windows.Forms.Form", ComponentCreator.TypeNames[0]);
		}
		
		/// <summary>
		/// The System.Drawing.Size type name should have been looked up by the PythonFormWalker when
		/// parsing the InitializeComponent method. Note that this is the second type that is looked up.
		/// The first lookup is the base class type.
		/// </summary>
		[Test]
		public void TypeNameLookedUp()
		{
			Assert.AreEqual("System.Drawing.Size", ComponentCreator.TypeNames[1]);
		}

		[Test]
		public void OneObjectCreated()
		{
			Assert.AreEqual(1, ComponentCreator.CreatedInstances.Count);
		}
		
		[Test]
		public void InstanceType()
		{
			List<object> args = new List<object>();
			int width = 300;
			int height = 400;
			args.Add(width);
			args.Add(height);
			
			CreatedInstance expectedInstance = new CreatedInstance(typeof(Size), args, null, false);
			Assert.AreEqual(expectedInstance, ComponentCreator.CreatedInstances[0]);
		}		
	}
}
