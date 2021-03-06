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

using ICSharpCode.Reports.Core.Interfaces;
using System;
using System.Drawing;
using System.Drawing.Printing;

/// <summary>
/// Derived from PrintDocument to have more control about printing
/// </summary>
/// <remarks>
/// 	created by - Forstmeier Peter
/// 	created on - 21.11.2004 14:47:20
/// </remarks>

namespace ICSharpCode.Reports.Core {
	
	
	public class ReportDocument : PrintDocument {
		///<summary>
		/// Fired just before the DetailSection ist printed
		/// </summary>
		/// 
		public event EventHandler<ReportPageEventArgs> BodyStart;
		public event EventHandler<ReportPageEventArgs> BodyEnd;
		
		
		public event EventHandler<ReportPageEventArgs> RenderReportHeader;
		public event EventHandler<ReportPageEventArgs> RenderPageHeader;
		public event EventHandler<ReportPageEventArgs> RenderDetails;
		public event EventHandler<ReportPageEventArgs> RenderPageEnd;
		public event EventHandler<ReportPageEventArgs> RenderReportEnd;
		
		private bool detailsDone;
		private bool reportHasData;
		private ISinglePage singlePage;
		
		
		public ReportDocument():base() {
			this.reportHasData = true;
		}
		
		
		#region Overrides
		
		protected override void OnQueryPageSettings(QueryPageSettingsEventArgs e){
			base.OnQueryPageSettings(e);
		}
		
		
		protected override void OnBeginPrint(PrintEventArgs e){
			base.OnBeginPrint(e);
		}
		
		
		protected override void OnPrintPage(PrintPageEventArgs e){
			base.OnPrintPage(e);
			ReportPageEventArgs pea = new ReportPageEventArgs (e,this.singlePage,
			                                                   false,Point.Empty);
			
			// ReportHeader only on first page
			if (this.singlePage.PageNumber == 1) {
				if (this.RenderReportHeader != null) {
					this.RenderReportHeader(this,pea);
				}
			}
			
			// allway draw PageHeader
			
			if (this.RenderPageHeader != null) {
				this.RenderPageHeader (this,pea);
			}
			
			// Details

			if (BodyStart != null) {
				BodyStart (this,pea);
			}
			
			if (this.reportHasData == true) {
				if (this.RenderDetails != null) {
					this.RenderDetails(this,pea);
				}
				
				
				if (pea.ForceNewPage) {
					if (RenderPageEnd != null) {
						RenderPageEnd (this,pea);
						return;
					}
					pea.ForceNewPage = false;
					return;
				}
				
				if (BodyEnd != null) {
					BodyEnd (this,pea);
				}
			}
			
			
			// ReportFooter
			if (this.detailsDone) {
				this.RenderReportEnd(this,pea);
				if (pea.ForceNewPage) {
					e.HasMorePages = true;
					pea.ForceNewPage = false;

				}
			}

			
			//PageFooter
			if (RenderPageEnd != null) {
				RenderPageEnd (this,pea);
				e.HasMorePages = false;
				return;
			}
		}
		
		
		protected override void OnEndPrint(PrintEventArgs e){
			base.OnEndPrint(e);
		}
		
		#endregion
		
		
		#region Property's
		
		public bool DetailsDone {
			get {
				return this.detailsDone;
			}
			set {
				detailsDone = value;
			}
		}
		
		public bool ReportHasData {
			get {
				return this.reportHasData;
			}
			set {
				reportHasData = value;
				this.detailsDone = true;
			}
		}
		
		public ISinglePage SinglePage {
			get { return singlePage; }
			set { singlePage = value; }
		}
		
		#endregion
	}
}
