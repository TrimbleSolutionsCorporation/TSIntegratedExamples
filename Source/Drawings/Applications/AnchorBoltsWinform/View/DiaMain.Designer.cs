namespace AnchorBoltsWinform.View
{
    partial class DiaMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if(disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DiaMain));
            this.saveLoad1 = new Tekla.Structures.Dialog.UIControls.SaveLoad();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.ckd_AnFilerName = new System.Windows.Forms.CheckBox();
            this.ckd_DmSettingsName = new System.Windows.Forms.CheckBox();
            this.cbx_DmSettingsName = new System.Windows.Forms.ComboBox();
            this.ckd_DimOffset = new System.Windows.Forms.CheckBox();
            this.tbx_DimOffset = new System.Windows.Forms.TextBox();
            this.cbx_AnFilerName = new System.Windows.Forms.ComboBox();
            this.createApplyCancel1 = new Tekla.Structures.Dialog.UIControls.CreateApplyCancel();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // saveLoad1
            // 
            this.structuresExtender.SetAttributeName(this.saveLoad1, null);
            this.structuresExtender.SetAttributeTypeName(this.saveLoad1, null);
            this.saveLoad1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.structuresExtender.SetBindPropertyName(this.saveLoad1, null);
            this.saveLoad1.Dock = System.Windows.Forms.DockStyle.Top;
            this.saveLoad1.HelpFileType = Tekla.Structures.Dialog.UIControls.SaveLoad.HelpFileTypeEnum.General;
            this.saveLoad1.HelpKeyword = "";
            this.saveLoad1.HelpUrl = "";
            this.saveLoad1.Location = new System.Drawing.Point(0, 0);
            this.saveLoad1.Name = "saveLoad1";
            this.saveLoad1.SaveAsText = "";
            this.saveLoad1.Size = new System.Drawing.Size(483, 43);
            this.saveLoad1.TabIndex = 0;
            this.saveLoad1.UserDefinedHelpFilePath = null;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.structuresExtender.SetAttributeName(this.tableLayoutPanel1, null);
            this.structuresExtender.SetAttributeTypeName(this.tableLayoutPanel1, null);
            this.structuresExtender.SetBindPropertyName(this.tableLayoutPanel1, null);
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 36.43892F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 63.56108F));
            this.tableLayoutPanel1.Controls.Add(this.ckd_AnFilerName, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.ckd_DmSettingsName, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.cbx_DmSettingsName, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.ckd_DimOffset, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tbx_DimOffset, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.cbx_AnFilerName, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 49);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(483, 96);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // ckd_AnFilerName
            // 
            this.structuresExtender.SetAttributeName(this.ckd_AnFilerName, "AnFilerName");
            this.structuresExtender.SetAttributeTypeName(this.ckd_AnFilerName, "Boolean");
            this.ckd_AnFilerName.AutoSize = true;
            this.structuresExtender.SetBindPropertyName(this.ckd_AnFilerName, "Checked");
            this.ckd_AnFilerName.Checked = true;
            this.ckd_AnFilerName.CheckState = System.Windows.Forms.CheckState.Checked;
            this.structuresExtender.SetIsFilter(this.ckd_AnFilerName, true);
            this.ckd_AnFilerName.Location = new System.Drawing.Point(3, 3);
            this.ckd_AnFilerName.Name = "ckd_AnFilerName";
            this.ckd_AnFilerName.Size = new System.Drawing.Size(137, 17);
            this.ckd_AnFilerName.TabIndex = 0;
            this.ckd_AnFilerName.Text = "Anchor Bolt Filter Name";
            this.ckd_AnFilerName.UseVisualStyleBackColor = true;
            // 
            // ckd_DmSettingsName
            // 
            this.structuresExtender.SetAttributeName(this.ckd_DmSettingsName, "DmSettingsName");
            this.structuresExtender.SetAttributeTypeName(this.ckd_DmSettingsName, "Boolean");
            this.ckd_DmSettingsName.AutoSize = true;
            this.structuresExtender.SetBindPropertyName(this.ckd_DmSettingsName, "Checked");
            this.ckd_DmSettingsName.Checked = true;
            this.ckd_DmSettingsName.CheckState = System.Windows.Forms.CheckState.Checked;
            this.structuresExtender.SetIsFilter(this.ckd_DmSettingsName, true);
            this.ckd_DmSettingsName.Location = new System.Drawing.Point(3, 29);
            this.ckd_DmSettingsName.Name = "ckd_DmSettingsName";
            this.ckd_DmSettingsName.Size = new System.Drawing.Size(116, 17);
            this.ckd_DmSettingsName.TabIndex = 1;
            this.ckd_DmSettingsName.Text = "Dimension Settings";
            this.ckd_DmSettingsName.UseVisualStyleBackColor = true;
            // 
            // cbx_DmSettingsName
            // 
            this.structuresExtender.SetAttributeName(this.cbx_DmSettingsName, "DmSettingsName");
            this.structuresExtender.SetAttributeTypeName(this.cbx_DmSettingsName, "String");
            this.structuresExtender.SetBindPropertyName(this.cbx_DmSettingsName, "Text");
            this.cbx_DmSettingsName.FormattingEnabled = true;
            this.cbx_DmSettingsName.Location = new System.Drawing.Point(178, 29);
            this.cbx_DmSettingsName.Name = "cbx_DmSettingsName";
            this.cbx_DmSettingsName.Size = new System.Drawing.Size(121, 21);
            this.cbx_DmSettingsName.TabIndex = 3;
            // 
            // ckd_DimOffset
            // 
            this.structuresExtender.SetAttributeName(this.ckd_DimOffset, "DimOffset");
            this.structuresExtender.SetAttributeTypeName(this.ckd_DimOffset, "Boolean");
            this.ckd_DimOffset.AutoSize = true;
            this.structuresExtender.SetBindPropertyName(this.ckd_DimOffset, "Checked");
            this.ckd_DimOffset.Checked = true;
            this.ckd_DimOffset.CheckState = System.Windows.Forms.CheckState.Checked;
            this.structuresExtender.SetIsFilter(this.ckd_DimOffset, true);
            this.ckd_DimOffset.Location = new System.Drawing.Point(3, 55);
            this.ckd_DimOffset.Name = "ckd_DimOffset";
            this.ckd_DimOffset.Size = new System.Drawing.Size(129, 17);
            this.ckd_DimOffset.TabIndex = 4;
            this.ckd_DimOffset.Text = "Dimension Line Offset";
            this.ckd_DimOffset.UseVisualStyleBackColor = true;
            // 
            // tbx_DimOffset
            // 
            this.structuresExtender.SetAttributeName(this.tbx_DimOffset, "DimOffset");
            this.structuresExtender.SetAttributeTypeName(this.tbx_DimOffset, "Distance");
            this.structuresExtender.SetBindPropertyName(this.tbx_DimOffset, "Text");
            this.tbx_DimOffset.Location = new System.Drawing.Point(178, 55);
            this.tbx_DimOffset.Name = "tbx_DimOffset";
            this.tbx_DimOffset.Size = new System.Drawing.Size(121, 20);
            this.tbx_DimOffset.TabIndex = 5;
            // 
            // cbx_AnFilerName
            // 
            this.structuresExtender.SetAttributeName(this.cbx_AnFilerName, "AnFilerName");
            this.structuresExtender.SetAttributeTypeName(this.cbx_AnFilerName, "String");
            this.structuresExtender.SetBindPropertyName(this.cbx_AnFilerName, "Text");
            this.cbx_AnFilerName.FormattingEnabled = true;
            this.cbx_AnFilerName.Location = new System.Drawing.Point(178, 3);
            this.cbx_AnFilerName.Name = "cbx_AnFilerName";
            this.cbx_AnFilerName.Size = new System.Drawing.Size(121, 21);
            this.cbx_AnFilerName.TabIndex = 6;
            // 
            // createApplyCancel1
            // 
            this.structuresExtender.SetAttributeName(this.createApplyCancel1, null);
            this.structuresExtender.SetAttributeTypeName(this.createApplyCancel1, null);
            this.structuresExtender.SetBindPropertyName(this.createApplyCancel1, null);
            this.createApplyCancel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.createApplyCancel1.Location = new System.Drawing.Point(0, 146);
            this.createApplyCancel1.Name = "createApplyCancel1";
            this.createApplyCancel1.Size = new System.Drawing.Size(483, 30);
            this.createApplyCancel1.TabIndex = 3;
            // 
            // DiaMain
            // 
            this.structuresExtender.SetAttributeName(this, null);
            this.structuresExtender.SetAttributeTypeName(this, null);
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.structuresExtender.SetBindPropertyName(this, null);
            this.ClientSize = new System.Drawing.Size(483, 176);
            this.Controls.Add(this.createApplyCancel1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.saveLoad1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DiaMain";
            this.ShowInTaskbar = true;
            this.Text = "Create Anchor Bolt Dimensions";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Tekla.Structures.Dialog.UIControls.SaveLoad saveLoad1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Tekla.Structures.Dialog.UIControls.CreateApplyCancel createApplyCancel1;
        private System.Windows.Forms.CheckBox ckd_AnFilerName;
        private System.Windows.Forms.CheckBox ckd_DmSettingsName;
        private System.Windows.Forms.ComboBox cbx_DmSettingsName;
        private System.Windows.Forms.CheckBox ckd_DimOffset;
        private System.Windows.Forms.TextBox tbx_DimOffset;
        private System.Windows.Forms.ComboBox cbx_AnFilerName;
    }
}