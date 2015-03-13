using System;
using System.Drawing;
using System.Windows.Forms;

using Sq1.Core;
using Sq1.Core.DoubleBuffered;

namespace Sq1.Charting.MultiSplit {
	public partial class MultiSplitContainerGeneric<PANEL_BASE>
			: UserControl
			//: UserControlDoubleBuffered
			where PANEL_BASE : Control {
		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
			if (this.DesignMode) return;
			if (Assembler.IsInitialized == false) return;
			if (Assembler.InstanceInitialized.MainFormDockFormsFullyDeserializedLayoutComplete == false) {
				return;
			}
			
			if (this.panels.Count == 0) return;
			this.DistributePanelsAndSplittersVertically();
			
			foreach (MultiSplitter splitter in this.splitters) {
		   		splitter.Width = base.Width;
		   		splitter.Invalidate();	// base.Invalidate() below doesn't reposition the dots
			}
			foreach (Control panel in this.panels) {
		   		panel.Width = base.Width;
		   		//TESTME panel.Invalidate();
			}
			base.Invalidate();
		}
//WHEN_INHERITED_FROM_REGULAR_USERCONTROL
		protected override void OnPaintBackground(PaintEventArgs e) {
			base.OnPaintBackground(e);
//WHEN_INHERITED_FROM_USERCONTROL_DOUBLEBUFFERED		protected override void OnPaintBackgroundDoubleBuffered(PaintEventArgs e) {
			if (base.DesignMode) return;
			try {
				for (int i=0; i<this.panels.Count; i++) {
					Control panel = this.panels[i];
					//panel.Invalidate();	// base.OnPaint() did it already
				}
			} catch (Exception ex) {
				string msg = "CONTROL.INVALIDATE()_IS_VERY_UNLIKELY_TO_THROW //MultiSplitContainerGeneric<PANEL_BASE>.OnPaintBackground()";
				Assembler.PopupException(msg, ex);
			}
		}
//WHEN_INHERITED_FROM_REGULAR_USERCONTROL
		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);	//DOUBLEBUFFERED_PARENT_DID_THAT
//WHEN_INHERITED_FROM_USERCONTROL_DOUBLEBUFFERED		protected override void OnPaintDoubleBuffered(PaintEventArgs e) {
			if (base.DesignMode) return;
			try {
				for (int i=0; i<this.panels.Count; i++) {
					//Control panel = this.panels[i];
					//panel.Invalidate();	// base.OnPaint() did it already
					
					if (i >= this.splitters.Count) {
						string msg = "YOU_GOT_MORE_(DESERIALIZED)_SPLITTERS_THAN_PANELS__MUST_BE_EQUAL  //MultiSplitContainerGeneric<PANEL_BASE>.OnPaint()";
						Assembler.PopupException(msg);
						continue;
					}
					MultiSplitter splitter = this.splitters[i];
					
					if (i == this.panelMouseIsOverNowIndexDropTarget) {
						splitter.BackColor = ColorBackgroundSliderDroppingTarget;
					} else {
						splitter.BackColor = ColorBackgroundSliderRegular;
					}
					//splitter.Invalidate();
				}
				//Point location = new Point(0, 0);
				//e.Graphics.DrawString(this.panelText, this.Font, SystemBrushes.ControlText, location);
			} catch (Exception ex) {
				string msg = "CONTROL.INVALIDATE()_IS_VERY_UNLIKELY_TO_THROW //MultiSplitContainerGeneric<PANEL_BASE>.OnPaint()";
				Assembler.PopupException(msg, ex);
			}
		}
		//protected override void OnMouseMove(MouseEventArgs e) {
		//	// all my nested this.panels intercept their own OnMouseMove()s so I'm receiving only mouse-above-splitters here
		//	//Debugger.Break();
		//	//base.Invalidate();
		//}
		//protected override void OnMouseDown(MouseEventArgs e) {
		//	Debugger.Break();
		//}
		
		string panelText { get { return this.panelMouseIsOverNow != null ? this.panelMouseIsOverNow.Text : "<none>"; } }
		PANEL_BASE panelMouseIsOverNow;
		int panelMouseIsOverNowIndexDropTarget = -1;
		void panel_MouseEnter(object sender, EventArgs e) {
			if (panelMouseIsOverNow == sender) return;
			if (splitterStartedResizeOrDrag == null) { 
				//Debugger.Break();
				return;
			}
			PANEL_BASE panel = sender as PANEL_BASE;
			if (panel == null) {
				string msg = "I_MUST_BE_SUBSCRIBED_TO_TYPE " + typeof(PANEL_BASE) + "; got sender[" + sender.GetType() + "] //panel_MouseEnter()";
				Assembler.PopupException(msg);
				return;
			}
			this.panelMouseIsOverNow = panel;
			this.panelMouseIsOverNowIndexDropTarget = this.panels.IndexOf(panelMouseIsOverNow);
			MultiSplitter splitterForThisPanel = this.splitters[this.panelMouseIsOverNowIndexDropTarget];
			if (DebugSplitter) {
				//splitterAboveTarget.Text = this.splitterResizeOrDragStartedText + " targetIndex[" + this.panelMouseIsOverNowIndexDropTarget + "]";
				splitterForThisPanel.Text = " target[" + this.panelMouseIsOverNow.Text + "] index[" + this.panelMouseIsOverNowIndexDropTarget + "]";
			}
			//NOT_ENOUGH splitterForThisPanel.Invalidate();
			this.Invalidate();
		}		
		void panel_MouseLeave(object sender, EventArgs e) {
			string msig = " //panel_MouseLeave()";
			panelMouseIsOverNow = null;
			panelMouseIsOverNowIndexDropTarget = -1;
			
			PANEL_BASE panel = sender as PANEL_BASE;
			if (panel == null) {
				string msg = "I_MUST_BE_SUBSCRIBED_TO_TYPE " + typeof(PANEL_BASE)
					+ "; got sender[" + sender.GetType() + "]";
				Assembler.PopupException(msg + msig);
				return;
			}
			int panelIndex = this.panels.IndexOf(panel);
			if (panelIndex == -1 || this.splitters.Count < panelIndex) {
				string msg = "I_REFUSE_TO_INVALIDATE_SPLITTER CANT_FIND_SPLITTER_FOR_PANEL";
				Assembler.PopupException(msg + msig);
				return;
			}
			MultiSplitter splitterForThisPanel = this.splitters[panelIndex];
			if (this.DebugSplitter) {
				splitterForThisPanel.Text = "<leftThisPanel>";
			}
			//NOT_ENOUGH splitterForThisPanel.Invalidate();
			this.Invalidate();
		}
		
		bool splitterIsDraggingNow = false;
		bool splitterIsMovingNow = false;
		MultiSplitter splitterStartedResizeOrDrag;
		Point splitterStartedResizeOrDragPoint;
		string splitterStartedResizeOrDragText { get {
				string ret = "<notResizingOrDragging>";
				if (splitterStartedResizeOrDrag == null) return ret;
				//if (this.splitters.Count <= splitterResizeOrDragStarted) return ret;
				//MultiSplitter splitterMouseOverResizingOrDragging = this.splitters[splitterResizeOrDragStarted];
				//Control panel = splitterStartedResizeOrDrag.PanelAbove as Control;
				
				int index = this.splitters.IndexOf(splitterStartedResizeOrDrag);
				Control panel = this.panels[index];
				ret = panel != null ? panel.Text : "<SplitterLostItsPanel>";
				if (splitterIsDraggingNow) ret += " PanelDragging";
				if (splitterIsMovingNow) ret += " PanelResizing";
				return ret;
			} }
		MultiSplitter splitterMouseIsOverNow;
		void splitter_MouseMove(object sender, MouseEventArgs e) {			
			MultiSplitter splitter = sender as MultiSplitter;
			if (splitter == null) {
				string msg = "I_MUST_BE_SUBSCRIBED_TO_MultiSplitter, got sender[" + splitter.GetType() + "] //splitter_MouseMove()";
				Assembler.PopupException(msg);
				return;
			}
			if (DebugSplitter) {
				splitter.Text = this.splitterStartedResizeOrDragText;
			}
			
			if (this.splitterIsDraggingNow || this.splitterIsMovingNow) {
				Point mousePositionFromSplitContainerUpperLeft = new Point(e.X + this.splitterStartedResizeOrDragPoint.X, e.Y + this.splitterStartedResizeOrDragPoint.Y);
				if (this.splitterIsDraggingNow) this.splitter_Dragging(splitter, mousePositionFromSplitContainerUpperLeft);
				if (this.splitterIsMovingNow)	  this.splitter_Moving(splitter, mousePositionFromSplitContainerUpperLeft);
			} else {		//if (this.splitterIsDraggingNow == false && this.splitterIsResizingNow == false) {
				base.Cursor = (e.X <= GrabHandleWidth) ? Cursors.Hand : Cursors.HSplit;
			}

			if (DebugSplitter) {
				splitter.Text += " x:" + e.X + " y:" + e.Y;
				splitter.Invalidate();
				//this.Invalidate();
			}
			this.RaiseOnSplitterMovingNow(splitter);
		}
		void splitter_MouseEnter(object sender, EventArgs e) {
			MultiSplitter splitter = sender as MultiSplitter;
			if (splitter == null) {
				string msg = "I_MUST_BE_SUBSCRIBED_TO_MultiSplitter, got sender[" + splitter.GetType() + "] //splitter_MouseEnter()";
				Assembler.PopupException(msg);
				return;
			}
			splitterMouseIsOverNow = splitter;
			splitter.BackColor = ColorBackgroundSliderMouseOver;
			splitter.Invalidate();
		}
		void splitter_MouseLeave(object sender, EventArgs e) {
			MultiSplitter splitter = sender as MultiSplitter;
			if (splitter == null) {
				string msg = "I_MUST_BE_SUBSCRIBED_TO_MultiSplitter, got sender[" + splitter.GetType() + "] //splitter_MouseLeave()";
				Assembler.PopupException(msg);
				return;
			}
			if (splitterStartedResizeOrDrag != null) {
				if (DebugSplitter) {
					splitter.Text = "<leftThisSplitterWhileDragging>";
				}
				return;
			}

			splitterMouseIsOverNow = null;
			panelMouseIsOverNowIndexDropTarget = -1;
			base.Cursor = Cursors.Default;
			splitter.BackColor = ColorBackgroundSliderRegular;
			splitter.Invalidate();
				
			if (DebugSplitter) {
				splitter.Text = "<leftThisSplitter>";
				splitter.Invalidate();
				//this.Invalidate();
			}
		}

		void splitter_MouseDown(object sender, MouseEventArgs e) {
			MultiSplitter splitter = sender as MultiSplitter;
			if (splitter == null) {
				string msg = "I_MUST_BE_SUBSCRIBED_TO_MultiSplitter, got sender[" + splitter.GetType() + "] //splitter_MouseDown()";
				Assembler.PopupException(msg);
				return;
			}
			
			if (e.X <= GrabHandleWidth) {
				splitterIsDraggingNow = true;
			} else {
				splitterIsMovingNow = true;
			}
			splitterStartedResizeOrDrag = splitter;
			splitterStartedResizeOrDragPoint = new Point(splitter.Location.X + e.X, splitter.Location.Y + e.Y - splitter.Height);
			if (DebugSplitter) {
				splitter.Text = this.splitterStartedResizeOrDragText;
				splitter.Invalidate();		// makes the Text visible
			}
			if (splitterIsDraggingNow) {
				this.RaiseOnSplitterDragStarted(splitter);
			} else {
				this.RaiseOnSplitterMoveStarted(splitter);
			}
		}
		void splitter_MouseUp(object sender, MouseEventArgs e) {
			MultiSplitter splitter = sender as MultiSplitter;
			if (splitter == null) {
				string msg = "I_MUST_BE_SUBSCRIBED_TO_MultiSplitter, got sender[" + splitter.GetType() + "] //splitter_MouseUp()";
				Assembler.PopupException(msg);
				return;
			}
			if (splitterIsDraggingNow) {
				if (panelMouseIsOverNow == null) {
					string msg = "SINGLE_CLICK_ON_YELLOW_WITHOUT_DRAGGING?";
					Assembler.PopupException(msg, null, false);
					return;
				} else {
					int indexToMoveFrom = this.splitters.IndexOf(splitterStartedResizeOrDrag);
					int indexToMoveTo   = this.panels.IndexOf(panelMouseIsOverNow);
					if (indexToMoveFrom != indexToMoveTo) {
						this.panels.Move(indexToMoveFrom, indexToMoveTo);
						this.splitters.Move(indexToMoveFrom, indexToMoveTo);
						this.DistributePanelsAndSplittersVertically();
					}
				}
			}

			splitterStartedResizeOrDrag = null;
			splitterIsDraggingNow = false;
			splitterIsMovingNow = false;
			splitterStartedResizeOrDragPoint = new Point(-1, -1);
			
			if (DebugSplitter) {
				splitter.Text = this.splitterStartedResizeOrDragText;
				splitter.Invalidate();		// makes the Text visible
			}
			this.Invalidate();
			
			if (splitterIsDraggingNow) {
				this.RaiseOnSplitterDragEnded(splitter);
			} else {
				this.RaiseOnSplitterMoveEnded(splitter);
			}
		}
		
		void splitter_Dragging(MultiSplitter splitter, Point mousePositionFromSplitContainerUpperLeft) {
			//splitter.Text += " dragging";
			// I_HATE_HACKING_WINDOWS_FORMS mousedrag doesn't fire panel_MouseEnter()/panel_MouseLeave() even if the mouse is above them now, simulating it manually here
			Control panelMouseOvered = null; 
			//foreach (Control panel in this.panels) {
			for (int i=0; i<this.panels.Count; i++) {
				Control panel = this.panels[i];
				MultiSplitter splitterAbove = this.splitters[i];
				Point offsetted = new Point(mousePositionFromSplitContainerUpperLeft.X - panel.Location.X,
								  mousePositionFromSplitContainerUpperLeft.Y - panel.Location.Y);
				bool beyondUpperLeft = offsetted.X < 0 || offsetted.Y < 0;
				Rectangle panelAndSplitterRect = new Rectangle(0, 0, panel.Width, panel.Height + splitterAbove.Height);
				bool doesntContain = (beyondUpperLeft) ? true : (panelAndSplitterRect.Contains(offsetted) == false);
				if (beyondUpperLeft || doesntContain) {
					if (this.panelMouseIsOverNow != null) {
						string msg = "panel_MouseLeave should've set this.panelMouseIsOverNow=null";
						//Debugger.Break();
						continue;
					}
					//if (this.panelMouseIsOverNow == panel) {
						this.panel_MouseLeave(panel, null);
					//} else {
						//Debugger.Break();
					//}
				} else {
					if (panelMouseOvered != null) {
						string msg = "did you find the second panel mouseovered simultaneously???..";
						//Debugger.Break();
					}
					panelMouseOvered = panel;
				}
			}
			if (panelMouseOvered == null) return;
			if (this.panelMouseIsOverNow != null) {
				string msg = "panel_MouseLeave should've set this.panelMouseIsOverNow=null";
				//Debugger.Break();
				this.panel_MouseLeave(this.panelMouseIsOverNow, null);
				this.panelMouseIsOverNow = null;
			}
			this.panel_MouseEnter(panelMouseOvered, null);
			this.RaiseOnSplitterDraggingNow(splitter);
		}
		void splitter_Moving(MultiSplitter splitter, Point mousePositionFromSplitContainerUpperLeft) {
			int splitterIndex = this.splitters.IndexOf(splitter);
			if (splitterIndex == 0) return;
			
			Control panelBelow = this.panels[splitterIndex];
			Control panelAbove = this.panels[splitterIndex-1];
			
			// mouseMovingUp CAN BE NEGATIVE
			int mouseMovingUp = this.splitterStartedResizeOrDragPoint.Y - mousePositionFromSplitContainerUpperLeft.Y;

			if (mouseMovingUp > 0 && panelAbove.Height <= 5) return;
			if (mouseMovingUp < 0 && panelBelow.Height <= 5) return;
			if (mouseMovingUp < 0 && splitterIndex == this.splitters.Count - 1) {
				int baseHeight = base.Height;
				if (splitter.Location.Y + splitter.Height >= base.Height - 5) {
					return;
				}
			}

			int panelAboveMinimumHeight = this.MinimumPanelHeight;
			if (panelAbove.MinimumSize != default(Size)) {
				panelAboveMinimumHeight = panelAbove.MinimumSize.Height;
			}
			int panelAboveHeightProjected = panelAbove.Height - mouseMovingUp; 
			if (panelAboveHeightProjected < panelAboveMinimumHeight) {
				Cursor.Current = Cursors.No;
				return;
			}
			
			int panelBelowMinimumHeight = this.MinimumPanelHeight;
			if (panelBelow.MinimumSize != default(Size)) {
				panelBelowMinimumHeight = panelBelow.MinimumSize.Height;
			}
			int panelBelowHeightProjected = panelBelow.Height + mouseMovingUp;
			if (panelBelowHeightProjected < panelBelowMinimumHeight) {
				Cursor.Current = Cursors.No;
				return;
			}
			
			Cursor.Current = Cursors.HSplit;
			//SAME_DOWN if (mouseMovingUp > 0) {
				panelBelow.Height += mouseMovingUp;
				panelAbove.Height -= mouseMovingUp;
			//}
			this.DistributePanelsAndSplittersVertically();
			panelBelow.Invalidate();
			panelAbove.Invalidate();
			splitter.Invalidate();
			base.Invalidate();
			this.RaiseOnSplitterMovingNow(splitter);
		}
	}
}
