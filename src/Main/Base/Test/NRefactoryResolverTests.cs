using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.NRefactoryResolver;

namespace ICSharpCode.SharpDevelop.Tests
{
	[TestFixture]
	public class NRefactoryResolverTests
	{
		#region Test helper methods
		ICompilationUnit Parse(string fileName, string fileContent)
		{
			ICSharpCode.NRefactory.Parser.IParser p = ICSharpCode.NRefactory.Parser.ParserFactory.CreateParser(ICSharpCode.NRefactory.Parser.SupportedLanguages.CSharp, new StringReader(fileContent));
			p.Parse();
			IProjectContent pc = new CaseSensitiveProjectContent();
			lastPC = pc;
			NRefactoryASTConvertVisitor visitor = new NRefactoryASTConvertVisitor(pc);
			visitor.Visit(p.CompilationUnit, null);
			visitor.Cu.FileName = fileName;
			visitor.Cu.ErrorsDuringCompile = p.Errors.count > 0;
			visitor.Cu.Tag = p.CompilationUnit;
			foreach (IClass c in visitor.Cu.Classes) {
				pc.AddClassToNamespaceList(c);
			}
			
			return visitor.Cu;
		}
		
		IProjectContent lastPC;
		
		ICompilationUnit ParseVB(string fileName, string fileContent)
		{
			ICSharpCode.NRefactory.Parser.IParser p = ICSharpCode.NRefactory.Parser.ParserFactory.CreateParser(ICSharpCode.NRefactory.Parser.SupportedLanguages.VBNet, new StringReader(fileContent));
			p.Parse();
			IProjectContent pc = new CaseSensitiveProjectContent();
			lastPC = pc;
			NRefactoryASTConvertVisitor visitor = new NRefactoryASTConvertVisitor(pc);
			visitor.Visit(p.CompilationUnit, null);
			visitor.Cu.FileName = fileName;
			visitor.Cu.ErrorsDuringCompile = p.Errors.count > 0;
			visitor.Cu.Tag = p.CompilationUnit;
			foreach (IClass c in visitor.Cu.Classes) {
				pc.AddClassToNamespaceList(c);
			}
			
			return visitor.Cu;
		}
		
		void AddCompilationUnit(ICompilationUnit parserOutput, string fileName)
		{
			ParserService.UpdateParseInformation(parserOutput, fileName, false, false);
		}
		
		ResolveResult Resolve(string program, string expression, int line)
		{
			AddCompilationUnit(Parse("a.cs", program), "a.cs");
			
			NRefactoryResolver resolver = new NRefactoryResolver(ICSharpCode.NRefactory.Parser.SupportedLanguages.CSharp);
			return resolver.Resolve(expression,
			                        line, 0,
			                        "a.cs");
		}
		
		ResolveResult ResolveVB(string program, string expression, int line)
		{
			AddCompilationUnit(ParseVB("a.vb", program), "a.vb");
			
			NRefactoryResolver resolver = new NRefactoryResolver(ICSharpCode.NRefactory.Parser.SupportedLanguages.VBNet);
			return resolver.Resolve(expression,
			                        line, 0,
			                        "a.vb");
		}
		
		IProjectContent corLib;
		
		[TestFixtureSetUp]
		public void Init()
		{
			corLib = ProjectContentRegistry.GetMscorlibContent();
		}
		#endregion
		
		#region Test for old issues
		// Issue SD-291
		[Test]
		public void VBNetMultipleVariableDeclarationsTest()
		{
			string program = @"Class X
	Shared Sub Main
		Dim a, b As String
		
	End Sub
End Class
";
			ResolveResult result = ResolveVB(program, "a", 4);
			Assert.IsNotNull(result, "result");
			Assert.IsTrue(result is LocalResolveResult, "result is LocalResolveResult");
			Assert.AreEqual("System.String", result.ResolvedType.FullyQualifiedName);
			
			result = ResolveVB(program, "b", 4);
			Assert.IsNotNull(result, "result");
			Assert.IsTrue(result is LocalResolveResult, "result is LocalResolveResult");
			Assert.AreEqual("System.String", result.ResolvedType.FullyQualifiedName);
		}
		
		// Issue SD-258
		[Test]
		public void VBNetForeachLoopVariableTest()
		{
			string program = @"Class Test
	Shared Sub Main()
		For Each c As String In MyColl
			
		Next
	End Sub
End Class
";
			ResolveResult result = ResolveVB(program, "c", 4);
			Assert.IsNotNull(result, "result");
			Assert.IsTrue(result is LocalResolveResult, "result is LocalResolveResult");
			Assert.AreEqual("System.String", result.ResolvedType.FullyQualifiedName);
		}
		
		// Issue SD-265
		[Test]
		public void VBNetStaticMembersonObjectTest()
		{
			string program = @"Class X
	Sub Z()
		Dim a As String
		
	End Sub
End Class
";
			ResolveResult result = ResolveVB(program, "a", 4);
			Assert.IsNotNull(result, "result");
			ArrayList arr = result.GetCompletionData(lastPC);
			Assert.IsNotNull(arr, "arr");
			foreach (object o in arr) {
				if (o is IMember) {
					if (((IMember)o).FullyQualifiedName == "System.String.Empty")
						return;
				}
			}
			Assert.Fail("Static member empty not found on string instance!");
		}
		
		// Issue SD-217
		[Test]
		public void VBNetLocalArrayLookupTest()
		{
			string program = @"Module Main
	Sub Main()
		Dim t As String()
		
	End Sub
End Module
";
			ResolveResult result = ResolveVB(program, "t", 4);
			Assert.IsNotNull(result, "result");
			Assert.IsTrue(result is LocalResolveResult, "result is LocalResolveResult");
			
			ArrayList arr = result.GetCompletionData(lastPC);
			Assert.IsNotNull(arr, "arr");
			foreach (object o in arr) {
				if (o is IMember) {
					if (((IMember)o).FullyQualifiedName == "System.Array.Length")
						return;
				}
			}
			Assert.Fail("Length not found on array instance (resolve result was " + result.ResolvedType.ToString() + ")");
		}
		
		[Test]
		public void OuterclassPrivateFieldResolveTest()
		{
			string program = @"class A
{
	int myField;
	class B
	{
		void MyMethod(A a)
		{
		
		}
	}
}
";
			ResolveResult result = Resolve(program, "a", 8);
			Assert.IsNotNull(result, "result");
			Assert.IsTrue(result is LocalResolveResult, "result is LocalResolveResult");
			ArrayList arr = result.GetCompletionData(lastPC);
			Assert.IsNotNull(arr, "arr");
			foreach (object o in arr) {
				if (o is IField) {
					Assert.AreEqual("myField", ((IField)o).Name);
					return;
				}
			}
			Assert.Fail("private field not visible from inner class");
		}
		#endregion
		
		#region BasicTests
		[Test]
		public void InheritedInterfaceResolveTest()
		{
			string program = @"class A {
	void Method(IInterface1 a) {
		
	}
}
interface IInterface1 : IInterface2 {
	void Method1();
}
interface IInterface2 {
	void Method2();
}
";
			ResolveResult result = Resolve(program, "a", 3);
			Assert.IsNotNull(result, "result");
			Assert.IsTrue(result is LocalResolveResult, "result is LocalResolveResult");
			ArrayList arr = result.GetCompletionData(lastPC);
			Assert.IsNotNull(arr, "arr");
			Assert.AreEqual(2, arr.Count, "Number of CC results");
			foreach (IMethod m in arr) {
				if (m.Name == "Method2")
					return;
			}
			Assert.Fail("Method2 not found");
		}
		
		[Test]
		public void InvalidMethodCallTest()
		{
			string program = @"class A {
	void Method(string b) {
		
	}
}
";
			ResolveResult result = Resolve(program, "b.ThisMethodDoesNotExistOnString()", 3);
			Assert.IsNull(result, "result");
		}
		
		[Test]
		public void InvalidConstructorCallTest()
		{
			string program = @"class A {
	void Method() {
		
	}
}
";
			ResolveResult result = Resolve(program, "new ThisClassDoesNotExist()", 3);
			Assert.IsNull(result);
		}
		
		[Test]
		public void MethodCallTest()
		{
			string program = @"class A {
	void Method() {
		
	}
	
	int TargetMethod() {
		return 3;
	}
}
";
			ResolveResult result = Resolve(program, "TargetMethod()", 3);
			Assert.IsNotNull(result);
			Assert.IsTrue(result is MemberResolveResult);
			Assert.AreEqual("System.Int32", result.ResolvedType.FullyQualifiedName, "'TargetMethod()'");
		}
		
		[Test]
		public void ThisMethodCallTest()
		{
			string program = @"class A {
	void Method() {
		
	}
	
	int TargetMethod() {
		return 3;
	}
}
";
			ResolveResult result = Resolve(program, "this.TargetMethod()", 3);
			Assert.IsNotNull(result);
			Assert.IsTrue(result is MemberResolveResult);
			Assert.AreEqual("System.Int32", result.ResolvedType.FullyQualifiedName, "'this.TargetMethod()'");
		}
		
		[Test]
		public void OverloadLookupTest()
		{
			string program = @"class A {
	void Method() {
		
	}
	
	int Multiply(int a, int b) { return a * b; }
	double Multiply(double a, double b) { return a * b; }
}
";
			ResolveResult result = Resolve(program, "Multiply(1, 1)", 3);
			Assert.IsNotNull(result);
			Assert.IsTrue(result is MemberResolveResult, "'Multiply(1,1)' is MemberResolveResult");
			Assert.AreEqual("System.Int32", result.ResolvedType.FullyQualifiedName, "'Multiply(1,1)'");
			
			result = Resolve(program, "Multiply(1.0, 1.0)", 3);
			Assert.IsNotNull(result);
			Assert.IsTrue(result is MemberResolveResult, "'Multiply(1.0,1.0)' is MemberResolveResult");
			Assert.AreEqual("System.Double", result.ResolvedType.FullyQualifiedName, "'Multiply(1.0,1.0)'");
		}
		#endregion
	}
}
