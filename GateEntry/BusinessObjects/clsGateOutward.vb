
Public Class clsGateOutward
    Public Const Formtype As String = "MIGTOT"
    Dim objForm As SAPbouiCOM.Form
    Dim objCombo As SAPbouiCOM.ComboBox
    Dim Header, AttachLine As SAPbouiCOM.DBDataSource
    Dim objRS As SAPbobsCOM.Recordset
    Dim strSQL As String
    Dim objMatrix, oattachMatrix As SAPbouiCOM.Matrix
    Dim objHeader As SAPbouiCOM.DBDataSource
    Dim objLine As SAPbouiCOM.DBDataSource
    Dim objlink As SAPbouiCOM.LinkedButton
    Dim objedit As SAPbouiCOM.EditText
    Dim FinDate(2) As String

    Public Sub LoadScreen()

        objForm = objAddOn.objUIXml.LoadScreenXML("Outward.xml", Mukesh.SBOLib.UIXML.enuResourceType.Embeded, Formtype)
        objHeader = objForm.DataSources.DBDataSources.Item("@MIGTOT")
        AttachLine = objForm.DataSources.DBDataSources.Item("@MIGTOT2")
        InitForm(objForm.UniqueID)
        objAddOn.objGenFunc.setReport(Formtype, "Gate Outward", objForm.TypeCount)
        ManageAttributes()
        objForm.Items.Item("51").Specific.ExpandType = SAPbouiCOM.BoExpandType.et_DescriptionOnly
        objForm.Items.Item("4").Specific.ExpandType = SAPbouiCOM.BoExpandType.et_DescriptionOnly
        objForm.Items.Item("20").Specific.ExpandType = SAPbouiCOM.BoExpandType.et_DescriptionOnly
        objForm.Items.Item("41").Specific.ExpandType = SAPbouiCOM.BoExpandType.et_DescriptionOnly
        objForm.Items.Item("48").Click()
        objForm.Visible = True
    End Sub

    Public Sub ItemEvent(ByVal FormUID As String, ByRef pval As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        Try
            If pval.ItemUID = "" Then Exit Sub
            objForm = objAddOn.objApplication.Forms.Item(FormUID)
            objCombo = objForm.Items.Item("8").Specific
            If pval.BeforeAction Then
                Select Case pval.EventType
                    Case SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST
                        If pval.ItemUID = "10" Then
                            'If objCombo.Selected.Value = "SI" Or objCombo.Selected.Value = "RT" Or objCombo.Selected.Value = "NR" Then
                            '    ChooseFromList_Filteration(FormUID, "BP_CFL", "CardType", "C")
                            'ElseIf objCombo.Selected.Value = "SR" Or objCombo.Selected.Value = "SC" Then
                            '    ChooseFromList_Filteration(FormUID, "BP_CFL", "CardType", "S")
                            'End If
                            If objCombo.Selected.Value = "SI" Then 'A/R Invoice
                                If objAddOn.HANA Then
                                    ChooseFromList_Filteration(FormUID, "BP_CFL", "CardType", "C", "select distinct ""CardCode"" from OINV")
                                Else
                                    ChooseFromList_Filteration(FormUID, "BP_CFL", "CardType", "C", "select distinct CardCode from OINV")
                                End If
                            ElseIf objCombo.Selected.Value = "NR" Then 'Delivery
                                If objAddOn.HANA Then
                                    ChooseFromList_Filteration(FormUID, "BP_CFL", "CardType", "C", "select distinct ""CardCode"" from ODLN Where ""U_TransTyp""='NRDC'")
                                Else
                                    ChooseFromList_Filteration(FormUID, "BP_CFL", "CardType", "C", "select distinct CardCode from ODLN Where U_TransTyp='NRDC'")
                                End If
                            ElseIf objCombo.Selected.Value = "RT" Then 'Delivery
                                If objAddOn.HANA Then
                                    ChooseFromList_Filteration(FormUID, "BP_CFL", "CardType", "C", "select distinct ""CardCode"" from ODLN Where ""U_TransTyp""='RDC'")
                                Else
                                    ChooseFromList_Filteration(FormUID, "BP_CFL", "CardType", "C", "select distinct CardCode from ODLN Where U_TransTyp='RDC'")
                                End If
                            ElseIf objCombo.Selected.Value = "SR" Then 'Goods Return
                                If objAddOn.HANA Then
                                    ChooseFromList_Filteration(FormUID, "BP_CFL", "CardType", "S", "select distinct ""CardCode"" from ORPD")
                                Else
                                    ChooseFromList_Filteration(FormUID, "BP_CFL", "CardType", "S", "select distinct CardCode from ORPD")
                                End If
                            ElseIf objCombo.Selected.Value = "SC" Then 'AP Credit Memo
                                If objAddOn.HANA Then
                                    ChooseFromList_Filteration(FormUID, "BP_CFL", "CardType", "S", "select distinct ""CardCode"" from ORPC")
                                Else
                                    ChooseFromList_Filteration(FormUID, "BP_CFL", "CardType", "S", "select distinct CardCode from ORPC")
                                End If
                            Else
                                Dim oCFL As SAPbouiCOM.ChooseFromList = objForm.ChooseFromLists.Item("BP_CFL")
                                Dim oEmptyConds As New SAPbouiCOM.Conditions
                                oCFL.SetConditions(oEmptyConds)
                            End If
                        End If
                    Case SAPbouiCOM.BoEventTypes.et_VALIDATE
                        If pval.ItemUID = "36" And pval.ColUID = "9" Then ' line total calculation
                            If Not QtyValidation(FormUID, pval.Row) Then
                                BubbleEvent = False
                                Exit Sub
                            End If
                            If objMatrix.Columns.Item("4").Cells.Item(pval.Row).Specific.String = "" Then Exit Sub
                            If objMatrix.Columns.Item("9").Cells.Item(pval.Row).Specific.String = "" Then objMatrix.Columns.Item("9").Cells.Item(pval.Row).Click() : Exit Sub
                            If CDbl(objMatrix.Columns.Item("9").Cells.Item(pval.Row).Specific.String) <= 0 Then
                                objAddOn.objApplication.StatusBar.SetText("Value in ""Quantity"" cannot be zero.  Line: " & pval.Row, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
                                objMatrix.Columns.Item("9").Cells.Item(pval.Row).Specific.String = CDbl(1)
                                'objMatrix.Columns.Item("9").Cells.Item(pval.Row).Click() : BubbleEvent = False : Exit Sub
                            End If
                        End If
                        If (pval.ItemUID = "8" Or pval.ItemUID = "10") And pval.ItemChanged = True Then
                            If objMatrix.VisualRowCount > 0 Then objMatrix.Clear() : If objForm.Items.Item("10").Specific.String = "" Then objForm.Items.Item("12").Specific.String = ""
                        End If
                    Case SAPbouiCOM.BoEventTypes.et_CLICK
                        objForm = objAddOn.objApplication.Forms.Item(FormUID)
                        If pval.ItemUID = "1" Then
                            If pval.BeforeAction = True And (objForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE Or objForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE) Then
                                If Validate(FormUID) = False Then
                                    '     System.Media.SystemSounds.Asterisk.Play()
                                    BubbleEvent = False
                                    ' objAddOn.objApplication.SetStatusBarMessage("ItemEvent")
                                    Exit Sub
                                End If
                            End If
                            If objForm.Mode <> SAPbouiCOM.BoFormMode.fm_FIND_MODE Then
                                objAddOn.objGenFunc.RemoveLastrow(objMatrix, "4")
                            End If
                        End If
                    Case SAPbouiCOM.BoEventTypes.et_MATRIX_LINK_PRESSED
                        If pval.ColUID = "2" Then
                            Dim ColItem As SAPbouiCOM.Column
                            ColItem = objMatrix.Columns.Item("2")
                            objlink = ColItem.ExtendedObject
                            If objMatrix.Columns.Item("1A").Cells.Item(pval.Row).Specific.String = "13" Then 'A/R Invoice
                                objlink.LinkedObjectType = "13"
                            ElseIf objMatrix.Columns.Item("1A").Cells.Item(pval.Row).Specific.String = "15" Then 'Delivery
                                objlink.LinkedObjectType = "15"
                            ElseIf objMatrix.Columns.Item("1A").Cells.Item(pval.Row).Specific.String = "19" Then 'A/P Credit Memo
                                objlink.LinkedObjectType = "19"
                            ElseIf objMatrix.Columns.Item("1A").Cells.Item(pval.Row).Specific.String = "60" Then 'Goods Issue
                                objlink.LinkedObjectType = "60"
                            ElseIf objMatrix.Columns.Item("1A").Cells.Item(pval.Row).Specific.String = "21" Then 'Goods Return
                                objlink.LinkedObjectType = "21"
                            ElseIf objMatrix.Columns.Item("1A").Cells.Item(pval.Row).Specific.String = "67" Then 'Inventory Transfer
                                objlink.LinkedObjectType = "67"
                            Else
                                BubbleEvent = False
                            End If
                        End If
                    Case SAPbouiCOM.BoEventTypes.et_KEY_DOWN
                        If objForm.Mode = SAPbouiCOM.BoFormMode.fm_FIND_MODE Then Exit Sub
                        If pval.ItemUID = "21" Then
                            BubbleEvent = False
                        End If
                        'If (pval.ItemUID = "8" Or pval.ItemUID = "10") And (objForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE Or objForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE) Then
                        '    BubbleEvent = False
                        'End If
                End Select
            Else
                Select Case pval.EventType
                    Case SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED
                        If pval.ItemUID = "37" Then ' copy From
                            CopyFrom(FormUID)
                        ElseIf pval.ItemUID = "38" Then
                            CopyToStockTransfer(FormUID)
                        ElseIf pval.ItemUID = "1" And pval.ActionSuccess And objForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE Then
                            InitForm(FormUID)
                        End If

                    Case SAPbouiCOM.BoEventTypes.et_LOST_FOCUS
                        If pval.ItemUID = "36" And pval.ColUID = "9" Then ' line total calculation
                            LineTotalCalc(FormUID, pval.Row)
                        End If
                        If pval.ItemUID = "23" Then 'DocDate
                            If objForm.Mode <> SAPbouiCOM.BoFormMode.fm_ADD_MODE Then Exit Sub
                            objedit = objForm.Items.Item("23").Specific
                            Try
                                If FinDate(1) = "" Then Exit Sub
                                If FinDate(0) <> FinDate(1) Then 'Year(Now)
                                    objAddOn.objApplication.MessageBox("Newly entered posting date relates to another posting period. Do you want to Continue?", 2, "Yes", "No")
                                    objCombo = objForm.Items.Item("20").Specific
                                    For i As Integer = objCombo.ValidValues.Count - 1 To 0 Step -1
                                        objCombo.ValidValues.Remove(i, SAPbouiCOM.BoSearchKey.psk_Index)
                                    Next
                                    If objAddOn.HANA Then
                                        strSQL = "select ""Series"",""SeriesName"" From NNM1 where ""ObjectCode""='" & Formtype & "' and ""Indicator""=(select Top 1 ""Indicator""  from OFPR where '" & objedit.Value & "' between ""F_RefDate"" and ""T_RefDate"")  "
                                    Else
                                        strSQL = "select Series,SeriesName From NNM1 where ObjectCode='" & Formtype & "' and Indicator=(select Top 1 Indicator  from OFPR where '" & objedit.Value & "' between F_RefDate and T_RefDate)  "
                                    End If
                                    objRS = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                    objRS.DoQuery(strSQL)
                                    If objRS.RecordCount > 0 Then
                                        For Rec As Integer = 0 To objRS.RecordCount - 1
                                            objCombo.ValidValues.Add(objRS.Fields.Item(0).Value.ToString, objRS.Fields.Item(1).Value.ToString)
                                            objRS.MoveNext()
                                        Next
                                    End If
                                    If objCombo.ValidValues.Count > 0 Then objCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index)
                                End If
                            Catch ex As Exception
                                'objAddOn.objApplication.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                            End Try
                        End If
                    Case SAPbouiCOM.BoEventTypes.et_FORM_RESIZE
                        objMatrix.AutoResizeColumns()
                    Case SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST
                        If pval.ItemUID = "10" Then 'Partyid
                            ChooseFromListBP(FormUID, pval)
                        End If
                    Case SAPbouiCOM.BoEventTypes.et_COMBO_SELECT
                        If objForm.Mode = SAPbouiCOM.BoFormMode.fm_FIND_MODE Or objForm.Items.Item("25").Specific.Selected.Value = "C" Then Exit Sub
                        If pval.ItemUID = "41" Then
                            objCombo = objForm.Items.Item("41").Specific
                            If objCombo.Selected.Value = "1" Then
                                objForm.Items.Item("43").Specific.String = DateTime.Now.ToString("HH:mm")
                            End If
                        ElseIf pval.ItemUID = "8" Then
                            objCombo = objForm.Items.Item("8").Specific
                            objForm.Items.Item("47").Specific.String = GetTransaction_Type(FormUID, objCombo.Selected.Value)
                            If pval.ItemChanged = True Then objForm.Items.Item("10").Specific.String = "" : objForm.Items.Item("12").Specific.String = ""
                        ElseIf pval.ItemUID = "20" Then
                            If objForm.Mode <> SAPbouiCOM.BoFormMode.fm_ADD_MODE Then Exit Sub
                            objCombo = objForm.Items.Item("20").Specific
                            Dim StrDocNum = objForm.BusinessObject.GetNextSerialNumber(Trim(objForm.Items.Item("20").Specific.Selected.value), objForm.BusinessObject.Type)
                            objForm.DataSources.DBDataSources.Item("@MIGTOT").SetValue("DocNum", 0, StrDocNum)
                        End If
                    Case SAPbouiCOM.BoEventTypes.et_CLICK
                        If pval.ItemUID = "36" Then
                            objMatrix.SelectRow(pval.Row, True, False)
                        ElseIf pval.ItemUID = "Btnbrowse" Then
                            If objForm.Items.Item(pval.ItemUID).Enabled = False Or objForm.Mode = SAPbouiCOM.BoFormMode.fm_FIND_MODE Then Exit Sub
                            If objAddOn.objGenFunc.SetAttachMentFile(objForm, objHeader, oattachMatrix, AttachLine) = False Then
                                BubbleEvent = False
                            End If
                            If oattachMatrix.GetNextSelectedRow(0, SAPbouiCOM.BoOrderType.ot_SelectionOrder) = -1 Then
                                objForm.Items.Item("Btndisp").Enabled = False
                                objForm.Items.Item("Btndel").Enabled = False
                            End If
                        ElseIf pval.ItemUID = "Btndisp" Then
                            If objForm.Items.Item(pval.ItemUID).Enabled = False Then Exit Sub
                            If pval.ActionSuccess Then objAddOn.objGenFunc.OpenAttachment(oattachMatrix, AttachLine, pval.Row)
                        ElseIf pval.ItemUID = "Btndel" Then
                            If objForm.Items.Item(pval.ItemUID).Enabled = False Then Exit Sub
                            If pval.ActionSuccess Then
                                objAddOn.objGenFunc.DeleteRowAttachment(objForm, oattachMatrix, AttachLine, oattachMatrix.GetNextSelectedRow(0, SAPbouiCOM.BoOrderType.ot_SelectionOrder))
                            End If
                        ElseIf pval.ItemUID = "mtxattach" Then
                            oattachMatrix.SelectRow(pval.Row, True, False)
                            If pval.Row > 0 Then
                                If oattachMatrix.IsRowSelected(pval.Row) Then
                                    objForm.Items.Item("Btndisp").Enabled = True
                                    objForm.Items.Item("Btndel").Enabled = True
                                End If
                            End If
                        End If
                    Case SAPbouiCOM.BoEventTypes.et_KEY_DOWN
                        If pval.ItemUID = "36" Then
                            Dim ColID As Integer = objMatrix.GetCellFocus().ColumnIndex
                            If pval.CharPressed = 38 And pval.Modifiers = SAPbouiCOM.BoModifiersEnum.mt_None Then  'up
                                objMatrix.SetCellFocus(pval.Row - 1, ColID)
                                objMatrix.SelectRow(pval.Row - 1, True, False)
                            ElseIf pval.CharPressed = 40 And pval.Modifiers = SAPbouiCOM.BoModifiersEnum.mt_None Then 'down
                                objMatrix.SetCellFocus(pval.Row + 1, ColID)
                                objMatrix.SelectRow(pval.Row + 1, True, False)
                            End If
                        End If
                    Case SAPbouiCOM.BoEventTypes.et_VALIDATE
                        If pval.ItemUID = "23" Then 'DocDate
                            If pval.ItemChanged = True And pval.InnerEvent = False Then
                                If FinDate(1) = "" Then FinDate(1) = Year(Now) Else FinDate(1) = FinDate(0)
                                objedit = objForm.Items.Item("23").Specific
                                Dim DocDate As Date = Date.ParseExact(objedit.Value, "yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo)
                                FinDate(0) = DocDate.Year
                                If FinDate(0) = FinDate(1) Then FinDate(1) = ""
                            End If
                        End If
                End Select

            End If
        Catch ex As Exception

        End Try
    End Sub

    Public Sub MenuEvent(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean)
        objMatrix = objForm.Items.Item("36").Specific
        Try
            Select Case pVal.MenuUID
                Case "1282"
                    If pVal.BeforeAction = False Then InitForm(objAddOn.objApplication.Forms.ActiveForm.UniqueID)
                    objMatrix.Item.Enabled = True
                Case "1293"  'delete Row
                    DeleteRow(objMatrix, "@MIGTOT1")
                    'For i As Integer = objMatrix.VisualRowCount To 1 Step -1
                    '    objMatrix.Columns.Item("0").Cells.Item(i).Specific.String = i
                    'Next
                    'Case "1289"
                    '    If pVal.BeforeAction = False Then Me.UpdateMode()
                    'Case "1293"
                    'Case "1281"
                    '    If pVal.BeforeAction = False Then

                    '   End If
                Case "1281"
                    objMatrix.Item.Enabled = False
                    objForm.Items.Item("46").Enabled = True
                    objForm.Items.Item("47").Enabled = True
                    objForm.Items.Item("Btnbrowse").Enabled = False
                    objForm.Items.Item("Btndisp").Enabled = False
                    objForm.Items.Item("Btndel").Enabled = False
            End Select
        Catch ex As Exception
            objAddOn.objApplication.StatusBar.SetText("Menu Event Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
        Finally
        End Try
    End Sub

    Public Sub FormDataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean)
        Try
            If BusinessObjectInfo.BeforeAction Then
                Select Case BusinessObjectInfo.EventType
                    Case SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD, SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE
                        If Validate(BusinessObjectInfo.FormUID) Then
                        Else
                            BubbleEvent = False
                            Exit Sub
                        End If
                End Select
            Else
                Select Case BusinessObjectInfo.EventType
                    Case SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD
                        If BusinessObjectInfo.ActionSuccess = True Then
                            objRS = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            If objCombo.Selected.Value = "SI" Then ' Sales Invoice
                                If objAddOn.HANA Then
                                    strSQL = "Update T0 Set T0.""U_geentry""='" & objHeader.GetValue("DocEntry", 0) & "' From INV1 T0 Left join ""@MIGTOT1"" T1 On T0.""DocEntry""=T1.""U_basentry"" and T0.""LineNum""=T1.""U_baseline"" Where T1.""DocEntry""=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                    strSQL = "Update T0 Set T0.""U_gever""='" & objHeader.GetValue("DocEntry", 0) & "' From OINV T0 Left join ""@MIGTOT1"" T1 On T0.""DocEntry""=T1.""U_basentry"" Where T1.""DocEntry""=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                Else
                                    strSQL = "Update T0 Set T0.U_geentry='" & objHeader.GetValue("DocEntry", 0) & "' From INV1 T0 Left join [@MIGTOT1] T1 On T0.DocEntry=T1.U_basentry and T0.LineNum=T1.U_baseline Where T1.DocEntry=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                    strSQL = "Update T0 Set T0.U_gever='" & objHeader.GetValue("DocEntry", 0) & "' From OINV T0 Left join [@MIGTOT1] T1 On T0.DocEntry=T1.U_basentry Where T1.DocEntry=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                End If

                            ElseIf objCombo.Selected.Value = "SR" Then 'Goods Return
                                If objAddOn.HANA Then
                                    strSQL = "Update T0 Set ""U_geentry""='" & objHeader.GetValue("DocEntry", 0) & "' From RPD1 T0 Left join ""@MIGTOT1"" T1 On T0.""DocEntry""=T1.""U_basentry"" and T0.""LineNum""=T1.""U_baseline"" Where T1.""DocEntry""=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                    strSQL = "Update T0 Set ""U_gever""='" & objHeader.GetValue("DocEntry", 0) & "' From ORPD T0 Left join ""@MIGTOT1"" T1 On T0.""DocEntry""=T1.""U_basentry"" Where T1.""DocEntry""=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                Else
                                    strSQL = "Update T0 Set U_geentry='" & objHeader.GetValue("DocEntry", 0) & "' From RPD1 T0 Left join [@MIGTOT1] T1 On T0.DocEntry=T1.U_basentry and T0.LineNum=T1.U_baseline Where T1.DocEntry=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                    strSQL = "Update T0 Set U_gever='" & objHeader.GetValue("DocEntry", 0) & "' From ORPD T0 Left join [@MIGTOT1] T1 On T0.DocEntry=T1.U_basentry Where T1.DocEntry=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                End If

                            ElseIf objCombo.Selected.Value = "RT" Or objCombo.Selected.Value = "NR" Then 'Delivery
                                If objAddOn.HANA Then
                                    strSQL = "Update T0 Set ""U_geentry""='" & objHeader.GetValue("DocEntry", 0) & "' From DLN1 T0 Left join ""@MIGTOT1"" T1 On T0.""DocEntry""=T1.""U_basentry"" and T0.""LineNum""=T1.""U_baseline"" Where T1.""DocEntry""=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                    strSQL = "Update T0 Set ""U_gever""='" & objHeader.GetValue("DocEntry", 0) & "' From ODLN T0 Left join ""@MIGTOT1"" T1 On T0.""DocEntry""=T1.""U_basentry"" Where T1.""DocEntry""=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                Else
                                    strSQL = "Update T0 Set U_geentry='" & objHeader.GetValue("DocEntry", 0) & "' From DLN1 T0 Left join [@MIGTOT1] T1 On T0.DocEntry=T1.U_basentry and T0.LineNum=T1.U_baseline Where T1.DocEntry=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                    strSQL = "Update T0 Set U_gever='" & objHeader.GetValue("DocEntry", 0) & "' From ODLN T0 Left join [@MIGTOT1] T1 On T0.DocEntry=T1.U_basentry Where T1.DocEntry=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                End If
                                If objCombo.Selected.Value = "NR" Then
                                    If objAddOn.HANA Then
                                        strSQL = "Select distinct T0.""DocEntry"" from DLN1 T0 Left Join ""@MIGTOT1"" T1 On T0.""DocEntry""=T1.""U_basentry"" and T0.""ItemCode""=T1.""U_itemcode"" and T0.""LineNum""=T1.""U_baseline"""
                                        strSQL += vbCrLf + "Where T1.""DocEntry""=" & objHeader.GetValue("DocEntry", 0) & " group by T0.""DocEntry"",T0.""ItemCode"" having sum(T0.""Quantity"")-ifnull(sum(T1.""U_qty""),0)=0"
                                    Else
                                        strSQL = "Select distinct T0.DocEntry from DLN1 T0 Left Join @MIGTOT1 T1 On T0.DocEntry=T1.U_basentry and T0.ItemCode=T1.U_itemcode and T0.LineNum=T1.U_baseline"
                                        strSQL += vbCrLf + "Where T1.DocEntry=" & objHeader.GetValue("DocEntry", 0) & " group by T0.DocEntry,T0.ItemCode having sum(T0.Quantity)-isnull(sum(T1.U_qty),0)=0"
                                    End If
                                    objRS.DoQuery(strSQL)
                                    For index = 0 To objRS.RecordCount - 1
                                        If objRS.Fields.Item(0).Value.ToString() <> "" Then
                                            Close_Delivery(objRS.Fields.Item(0).Value.ToString())
                                        End If
                                        objRS.MoveNext()
                                    Next
                                End If
                            ElseIf objCombo.Selected.Value = "MO" Then 'Goods Issue
                                If objAddOn.HANA Then
                                    strSQL = "Update T0 Set ""U_geentry""='" & objHeader.GetValue("DocEntry", 0) & "' From IGE1 T0 Left join ""@MIGTOT1"" T1 On T0.""DocEntry""=T1.""U_basentry"" and T0.""LineNum""=T1.""U_baseline"" Where T1.""DocEntry""=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                    strSQL = "Update T0 Set ""U_gever""='" & objHeader.GetValue("DocEntry", 0) & "' From OIGE T0 Left join ""@MIGTOT1"" T1 On T0.""DocEntry""=T1.""U_basentry"" Where T1.""DocEntry""=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                Else
                                    strSQL = "Update T0 Set U_geentry='" & objHeader.GetValue("DocEntry", 0) & "' From IGE1 T0 Left join [@MIGTOT1] T1 On T0.DocEntry=T1.U_basentry and T0.LineNum=T1.U_baseline Where T1.DocEntry=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                    strSQL = "Update T0 Set U_gever='" & objHeader.GetValue("DocEntry", 0) & "' From OIGE T0 Left join [@MIGTOT1] T1 On T0.DocEntry=T1.U_basentry Where T1.DocEntry=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                End If

                            ElseIf objCombo.Selected.Value = "JW" Then 'Inventory Transfer
                                If objAddOn.HANA Then
                                    strSQL = "Update T0 Set ""U_geentry""='" & objHeader.GetValue("DocEntry", 0) & "' From WTR1 T0 Left join ""@MIGTOT1"" T1 On T0.""DocEntry""=T1.""U_basentry"" and T0.""LineNum""=T1.""U_baseline"" Where T1.""DocEntry""=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                    strSQL = "Update T0 Set ""U_gever""='" & objHeader.GetValue("DocEntry", 0) & "' From OWTR T0 Left join ""@MIGTOT1"" T1 On T0.""DocEntry""=T1.""U_basentry"" Where T1.""DocEntry""=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                Else
                                    strSQL = "Update T0 Set U_geentry='" & objHeader.GetValue("DocEntry", 0) & "' From WTR1 T0 Left join [@MIGTOT1] T1 On T0.DocEntry=T1.U_basentry and T0.LineNum=T1.U_baseline Where T1.DocEntry=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                    strSQL = "Update T0 Set U_gever='" & objHeader.GetValue("DocEntry", 0) & "' From OWTR T0 Left join [@MIGTOT1] T1 On T0.DocEntry=T1.U_basentry Where T1.DocEntry=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                End If

                            ElseIf objCombo.Selected.Value = "SC" Then 'A/P Credit Memo
                                If objAddOn.HANA Then
                                    strSQL = "Update T0 Set ""U_geentry""='" & objHeader.GetValue("DocEntry", 0) & "' From RPC1 T0 Left join ""@MIGTOT1"" T1 On T0.""DocEntry""=T1.""U_basentry"" and T0.""LineNum""=T1.""U_baseline"" Where T1.""DocEntry""=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                    strSQL = "Update T0 Set ""U_gever""='" & objHeader.GetValue("DocEntry", 0) & "' From ORPC T0 Left join ""@MIGTOT1"" T1 On T0.""DocEntry""=T1.""U_basentry"" Where T1.""DocEntry""=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                Else
                                    strSQL = "Update T0 Set U_geentry='" & objHeader.GetValue("DocEntry", 0) & "' From RPC1 T0 Left join [@MIGTOT1] T1 On T0.DocEntry=T1.U_basentry and T0.LineNum=T1.U_baseline Where T1.DocEntry=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                    strSQL = "Update T0 Set U_gever='" & objHeader.GetValue("DocEntry", 0) & "' From ORPC T0 Left join [@MIGTOT1] T1 On T0.DocEntry=T1.U_basentry Where T1.DocEntry=" & objForm.Items.Item("46").Specific.String & ""
                                    objRS.DoQuery(strSQL)
                                End If

                            End If

                        End If
                    Case SAPbouiCOM.BoEventTypes.et_FORM_DATA_LOAD
                        objMatrix.AutoResizeColumns()
                        If objForm.Items.Item("25").Specific.Selected.Value = "C" Then
                            objForm.Items.Item("41").Enabled = True
                            objForm.Items.Item("43").Enabled = True
                            objMatrix.Item.Enabled = False
                        Else
                            objMatrix.Item.Enabled = True
                        End If
                End Select
            End If
        Catch ex As Exception
            objAddOn.objApplication.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try

    End Sub

    Private Function Close_Delivery(ByVal DocEntry As String) As Boolean
        Try
            Dim document As SAPbobsCOM.Documents
            Dim Retval As Integer
            document = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes)

            If document.GetByKey(DocEntry) Then
                Retval = document.Close()
            End If

            If Retval <> 0 Then
                Return False
            Else
                Return True
            End If
        Catch ex As Exception
            objAddOn.objApplication.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            Return False
        End Try
    End Function


    Public Sub InitForm(ByVal FormUID As String)
        LoadType(FormUID)
        LoadSeries(FormUID)
        objForm = objAddOn.objApplication.Forms.Item(FormUID)
        objMatrix = objForm.Items.Item("36").Specific
        oattachMatrix = objForm.Items.Item("mtxattach").Specific
        objMatrix.Columns.Item("9").ColumnSetting.SumType = SAPbouiCOM.BoColumnSumType.bst_Auto
        objMatrix.Columns.Item("12").ColumnSetting.SumType = SAPbouiCOM.BoColumnSumType.bst_Auto
        objMatrix.AutoResizeColumns()
    End Sub

    Public Sub LoadSeries(ByVal FormUID As String)

        objForm = objAddOn.objApplication.Forms.Item(FormUID)
        objForm.Items.Item("45").Specific.String = "Created By " & objAddOn.objCompany.UserName & " on " & Now.ToString("dd/MMM/yyyy HH:mm:ss")
        '-----------Load Branch---------------
        If BranchFlag = "Y" Then
            objCombo = objForm.Items.Item("51").Specific
            If objCombo.ValidValues.Count = 0 Then
                If objAddOn.HANA Then
                    strSQL = "Select ""BPLId"",""BPLName"" from OBPL Where ""BPLId"" in (Select T0.""BPLId"" from OBPL T0 join USR6 T1 on T0.""BPLId""=T1.""BPLId"" where T1.""UserCode""='" & objAddOn.objCompany.UserName & "' and T0.""Disabled""<>'Y') Order by ""BPLName"" "
                Else
                    strSQL = "Select BPLId,BPLName from OBPL Where BPLId in (Select T0.BPLId from OBPL T0 join USR6 T1 on T0.BPLId=T1.BPLId where T1.UserCode='" & objAddOn.objCompany.UserName & "' and T0.Disabled<>'Y') Order by BPLName "
                End If
                objRS = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                objRS.DoQuery(strSQL)
                'objCombo.ValidValues.Add("-1", "All")
                While Not objRS.EoF
                    objCombo.ValidValues.Add(objRS.Fields.Item(0).Value, objRS.Fields.Item(1).Value)
                    objRS.MoveNext()
                End While
                Try
                    If DefaultBranch = "" Then
                        objAddOn.objApplication.Menus.Item("11010").Activate()
                        Dim tempmatrix As SAPbouiCOM.Matrix
                        tempmatrix = objAddOn.objApplication.Forms.ActiveForm.Items.Item("1320000003").Specific
                        DefaultBranch = tempmatrix.Columns.Item("1320000005").Cells.Item(tempmatrix.GetNextSelectedRow(0, SAPbouiCOM.BoOrderType.ot_SelectionOrder)).Specific.String
                        objAddOn.objApplication.Forms.ActiveForm.Close()
                    End If
                Catch ex As Exception
                End Try
                objRS = Nothing
            End If
            If DefaultBranch = "" Then objCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index) Else objCombo.Select(DefaultBranch, SAPbouiCOM.BoSearchKey.psk_ByDescription)
        Else
            objForm.Items.Item("51").Enabled = False
        End If
        '---------------- Load locations ------------
        objCombo = objForm.Items.Item("4").Specific
        If objCombo.ValidValues.Count = 0 Then
            If objAddOn.HANA Then
                strSQL = "select ""Code"", ""Location"" from OLCT order by ""Location"""
            Else
                strSQL = "select Code, Location from OLCT order by Location"
            End If

            objRS = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            objRS.DoQuery(strSQL)
            While Not objRS.EoF
                objCombo.ValidValues.Add(objRS.Fields.Item(0).Value, objRS.Fields.Item(1).Value)
                objRS.MoveNext()
            End While

            objRS = Nothing
        End If
        'objCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index)
        objCombo.Select(objCombo.ValidValues.Count - 1, SAPbouiCOM.BoSearchKey.psk_Index)
        objForm.Items.Item("16").Specific.String = DateTime.Now.ToString("HH:mm") 'DateTime.Now.ToShortTimeString
        '----------------Load series --------------
        objCombo = objForm.Items.Item("20").Specific
        objCombo.ValidValues.LoadSeries(Formtype, SAPbouiCOM.BoSeriesMode.sf_Add)
        If objCombo.ValidValues.Count > 0 Then objCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index)

        Dim StrDocNum
        Try
            StrDocNum = objForm.BusinessObject.GetNextSerialNumber(Trim(objForm.Items.Item("20").Specific.Selected.value), objForm.BusinessObject.Type)
        Catch ex As Exception
            objAddOn.objApplication.MessageBox("To generate this document, first define the numbering series in the Administration module")
            Exit Sub
        End Try
        objHeader = objForm.DataSources.DBDataSources.Item("@MIGTOT")
        objHeader.SetValue("DocNum", 0, StrDocNum)
        'objForm.DataSources.DBDataSources.Item("@MIGTOT").SetValue("DocNum", 0, StrDocNum)
        objForm.Items.Item("23").Specific.String = "A" ' current date
        If objForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE Then
            objCombo = objForm.Items.Item("8").Specific
            objCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index)
            objForm.Items.Item("47").Specific.String = GetTransaction_Type(FormUID, objCombo.Selected.Value)
        End If

        '------------ Load Security Name-------------
        objCombo = objForm.Items.Item("6").Specific
        If objCombo.ValidValues.Count = 0 Then
            If objAddOn.HANA Then
                strSQL = "SELECT T0.""empID"", T0.""firstName"" || ' ' || T0.""lastName"" as ""empName"", T1.""Name"" FROM OHEM T0 INNER JOIN OUDP T1 ON T0.""dept"" = T1.""Code"" WHERE T1.""Name"" ='Security' ;"
            Else
                strSQL = "SELECT T0.[empID], T0.[firstName] + ' ' + T0.[lastName] as empName, T1.[Name] FROM OHEM T0  INNER JOIN OUDP T1 ON T0.[dept] = T1.[Code] WHERE T1.[Name] ='Security'"
            End If

            objRS = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            objRS.DoQuery(strSQL)
            While Not objRS.EoF
                objCombo.ValidValues.Add(objRS.Fields.Item(0).Value, objRS.Fields.Item(1).Value)
                objRS.MoveNext()
            End While
            objRS = Nothing
        End If
    End Sub

    Private Sub LoadType(ByVal FormUID As String)
        objForm = objAddOn.objApplication.Forms.Item(FormUID)
        objCombo = objForm.Items.Item("8").Specific
        If objCombo.ValidValues.Count = 0 Then
            objCombo.ValidValues.Add("SI", "Sales Invoice")
            objCombo.ValidValues.Add("SR", "Supplier Return")
            'objCombo.ValidValues.Add("JO", "Job order DC")
            'objCombo.ValidValues.Add("SO", "Service Order DC")
            objCombo.ValidValues.Add("RT", "Returnable DC")
            objCombo.ValidValues.Add("NR", "Non-Returnable DC")
            'objCombo.ValidValues.Add("RW", "Rework DC")
            'objCombo.ValidValues.Add("RJ", "Rejection DC")
            objCombo.ValidValues.Add("SC", "Supplier Credit Memo")
            'objCombo.ValidValues.Add("ST", "Stock Transfer")
            'objCombo.ValidValues.Add("IU", "InterUnit DC")
            objCombo.ValidValues.Add("JW", "Job Work")
            objCombo.ValidValues.Add("MO", "Material Outward")
            'objCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index)
        End If
        objCombo.ExpandType = SAPbouiCOM.BoExpandType.et_DescriptionOnly
    End Sub

    Private Function GetTransaction_Type(ByVal FormUID As String, ByVal Type As String)
        Try
            Select Case Type
                Case "SI"
                    Type = "A/R Invoice"
                Case "SR"
                    Type = "Goods Return"
                Case "RT", "NR"
                    Type = "Delivery"
                Case "SC"
                    Type = "A/P Credit Memo"
                Case "MO"
                    Type = "Goods Issue"
                Case "JW"
                    Type = "Inventory Transfer"
                Case Else
                    Type = ""
            End Select
            Return Type
        Catch ex As Exception

        End Try
    End Function

    Private Sub ChooseFromListBP(ByVal FormUID As String, ByRef pval As SAPbouiCOM.ItemEvent)
        Dim CFLEvent As SAPbouiCOM.ChooseFromListEvent
        CFLEvent = pval
        Dim datatable As SAPbouiCOM.DataTable
        If CFLEvent.ChooseFromListUID = "BP_CFL" Then
            datatable = CFLEvent.SelectedObjects()
            Try
                objHeader.SetValue("U_partyid", 0, datatable.GetValue("CardCode", 0))
                objHeader.SetValue("U_partynm", 0, datatable.GetValue("CardName", 0))
            Catch ex As Exception

            End Try

        End If
    End Sub

    Private Sub LineTotalCalc0(ByVal FormUID As String, ByVal RowID As Integer)
        objForm = objAddOn.objApplication.Forms.Item(FormUID)
        objMatrix = objForm.Items.Item("36").Specific
        objLine = objForm.DataSources.DBDataSources.Item("@MIGTOT1")
        objMatrix.GetLineData(RowID)
        Dim linetotal As Double

        linetotal = CDbl(objMatrix.Columns.Item("9").Cells.Item(RowID).Specific.value) * CDbl(objLine.GetValue("U_unitpric", RowID - 1))

        objLine.SetValue("U_linetot", RowID - 1, linetotal)
        ' MsgBox(CStr(objLine.GetValue("U_linetot", RowID - 1)))
        objMatrix.SetLineData(RowID)
        objForm.Update()

    End Sub

    Private Sub LineTotalCalc(ByVal FormUID As String, ByVal RowID As Integer)
        objForm = objAddOn.objApplication.Forms.Item(FormUID)
        objMatrix = objForm.Items.Item("36").Specific
        objLine = objForm.DataSources.DBDataSources.Item("@MIGTOT1")
        'objMatrix.GetLineData(RowID)
        Dim linetotal As Double
        linetotal = CDbl(objMatrix.Columns.Item("9").Cells.Item(RowID).Specific.value) * CDbl(objMatrix.Columns.Item("11").Cells.Item(RowID).Specific.value)

        objMatrix.Columns.Item("12").Cells.Item(RowID).Specific.value = CStr(linetotal)
        'objLine.SetValue("U_linetot", RowID - 1, linetotal)
        ' MsgBox(CStr(objLine.GetValue("U_linetot", RowID - 1)))
        'objMatrix.SetLineData(RowID)
        objForm.Update()
        objForm.Refresh()
    End Sub

    Private Function Validate(ByVal FormUID As String) As Boolean
        objForm = objAddOn.objApplication.Forms.Item(FormUID)
        Try
            If BranchFlag = "Y" Then
                If objForm.Items.Item("51").Specific.Selected.Value Is Nothing Then
                    objAddOn.objApplication.SetStatusBarMessage("Please select Branch")
                    objForm.Items.Item("51").Click()
                    Return False
                End If
                objedit = objForm.Items.Item("23").Specific
                If objAddOn.HANA Then
                    strSQL = objAddOn.objGenFunc.getSingleValue("select 1 as ""Status"" From NNM1 where ""ObjectCode""='" & Formtype & "' and ""Indicator""=(select Top 1 ""Indicator""  from OFPR where '" & objedit.Value & "' between ""F_RefDate"" and ""T_RefDate"") and ""BPLId"" is not null ")
                Else
                    strSQL = objAddOn.objGenFunc.getSingleValue("select 1 as Status From NNM1 where ObjectCode='" & Formtype & "' and Indicator=(select Top 1 Indicator  from OFPR where '" & objedit.Value & "' between F_RefDate and T_RefDate) and BPLId is not null")
                End If
                If strSQL <> "" Then
                    If objAddOn.HANA Then
                        strSQL = objAddOn.objGenFunc.getSingleValue("select 1 as ""Status"" From NNM1 where ""ObjectCode""='" & Formtype & "' and ""Indicator""=(select Top 1 ""Indicator""  from OFPR where '" & objedit.Value & "' between ""F_RefDate"" and ""T_RefDate"") and ""BPLId""='" & objForm.Items.Item("51").Specific.Selected.Value & "' and ""Series""='" & objForm.Items.Item("20").Specific.Selected.Value & "'")
                    Else
                        strSQL = objAddOn.objGenFunc.getSingleValue("select 1 as Status From NNM1 where ObjectCode='" & Formtype & "' and Indicator=(select Top 1 Indicator  from OFPR where '" & objedit.Value & "' between F_RefDate and T_RefDate) and BPLId='" & objForm.Items.Item("51").Specific.Selected.Value & "' and Series='" & objForm.Items.Item("20").Specific.Selected.Value & "'")
                    End If
                    If strSQL = "" Then objAddOn.objApplication.SetStatusBarMessage("Cannot add transaction; numbering series assigned to another branch [Gate Entry Outward - Series] , '" & objForm.Items.Item("20").Specific.Selected.Description & "'") : Return False
                End If
            End If

            If Trim(objForm.Items.Item("4").Specific.Value) = "" Then
                objAddOn.objApplication.SetStatusBarMessage("Please fill up Location name")
                objForm.Items.Item("4").Click()
                Return False
            ElseIf Trim(objForm.Items.Item("23").Specific.String) = "" Then
                objAddOn.objApplication.SetStatusBarMessage("Please fill up Date")
                objForm.Items.Item("23").Click()
                Return False
            ElseIf Trim(objForm.Items.Item("6").Specific.Value) = "" Then
                objAddOn.objApplication.SetStatusBarMessage("Please fill up Security name")
                objForm.Items.Item("6").Click()
                Return False
            ElseIf Trim(objForm.Items.Item("10").Specific.String) = "" Then
                objAddOn.objApplication.SetStatusBarMessage("Please fill up Party details")
                objForm.Items.Item("10").Click()
                Return False
                'ElseIf Trim(objForm.Items.Item("14").Specific.String) = "" Then
                '    objAddOn.objApplication.SetStatusBarMessage("Please fill up No of packages")
                '    Return False
            ElseIf Trim(objForm.Items.Item("16").Specific.String) = "" Then
                objAddOn.objApplication.SetStatusBarMessage("Please fill up Out time")
                objForm.Items.Item("16").Click()
                Return False
                'ElseIf Trim(objForm.Items.Item("18").Specific.String) = "" Then
                '    objAddOn.objApplication.SetStatusBarMessage("Please fill up LR Number")
                '    Return False
                'ElseIf Trim(objForm.Items.Item("27").Specific.String) = "" Then
                '    objAddOn.objApplication.SetStatusBarMessage("Please fill up Gate Entry No")
                '    Return False

                'ElseIf Trim(objForm.Items.Item("29").Specific.String) = "" Then
                '    objAddOn.objApplication.SetStatusBarMessage("Please fill up Vehicle Name")
                '    Return False

            ElseIf Trim(objForm.Items.Item("31").Specific.String) = "" Then
                objAddOn.objApplication.SetStatusBarMessage("Please fill up Vehicle Number")
                objForm.Items.Item("31").Click()
                Return False

            ElseIf Trim(objForm.Items.Item("33").Specific.String) = "" Then
                objAddOn.objApplication.SetStatusBarMessage("Please fill up Transporter Name")
                objForm.Items.Item("33").Click()
                Return False
            End If
            objMatrix = objForm.Items.Item("36").Specific
            If objMatrix.RowCount = 0 Then
                objAddOn.objApplication.SetStatusBarMessage("Minimum one Line Item is Required.. ")
                Return False
            Else
                If objMatrix.Columns.Item("1").Cells.Item(1).Specific.value = "" Then
                    objAddOn.objApplication.SetStatusBarMessage("Minimum one Line Item is Required.. ")
                    Return False
                End If
            End If
        Catch ex As Exception
            objAddOn.objApplication.SetStatusBarMessage("Please check mandatory fields")
            Return False
        End Try
        Return True
    End Function

    Private Function QtyValidation(ByVal FormUID As String, ByVal RowID As Integer) As Boolean
        objForm = objAddOn.objApplication.Forms.Item(FormUID)
        objMatrix = objForm.Items.Item("36").Specific
        objLine = objForm.DataSources.DBDataSources.Item("@MIGTOT1")
        objMatrix.GetLineData(RowID)
        If CDbl(objMatrix.Columns.Item("9").Cells.Item(RowID).Specific.value) > CDbl(objLine.GetValue("U_pendqty", RowID - 1)) Then
            objAddOn.objApplication.SetStatusBarMessage("Quantity exceeds pending quantity", SAPbouiCOM.BoMessageTime.bmt_Short, True)
            Return False
        End If
        Return True
    End Function

    Private Sub CopyFrom(ByVal FormUID As String)
        Dim objForm As SAPbouiCOM.Form
        objForm = objAddOn.objApplication.Forms.Item(FormUID)
        objHeader = objForm.DataSources.DBDataSources.Item("@MIGTOT")
        objCombo = objForm.Items.Item("8").Specific
        If objForm.Items.Item("10").Specific.string = "" Then
            If objForm.Items.Item("8").Specific.Selected.Value <> "MO" And objForm.Items.Item("8").Specific.Selected.Value <> "JW" Then objAddOn.objApplication.MessageBox("Please select Party id") : Exit Sub
        End If
        objAddOn.objItemDetails.LoadScreen(Formtype, objForm.TypeCount, objCombo.Value, objForm.Items.Item("10").Specific.string, objHeader.GetValue("U_cutdate", 0), objHeader.GetValue("DocEntry", 0))
    End Sub

    Private Sub ARInvoiceCopyFrom(ByVal FormUID As String)
        Dim objForm As SAPbouiCOM.Form
        objForm = objAddOn.objApplication.Forms.Item(FormUID)


    End Sub

    Private Sub GoodsReturnCopyFrom(ByVal FormUID As String)
        Dim objForm As SAPbouiCOM.Form
        objForm = objAddOn.objApplication.Forms.Item(FormUID)

    End Sub

    Private Sub CopyToStockTransfer(ByVal FormUID As String)
        Dim objForm As SAPbouiCOM.Form
        objForm = objAddOn.objApplication.Forms.Item(FormUID)
        objMatrix = objForm.Items.Item("36").Specific
        objAddOn.objApplication.ActivateMenuItem("3080")
        'Matrix 23; form 940
        Dim StockTransferForm As SAPbouiCOM.Form
        StockTransferForm = objAddOn.objApplication.Forms.GetFormByTypeAndCount("940", 1)
        Dim STMatrix As SAPbouiCOM.Matrix
        STMatrix = StockTransferForm.Items.Item("23").Specific

        For i As Integer = 1 To objMatrix.RowCount
            STMatrix.Columns.Item("1").Cells.Item(i).Specific.String = objMatrix.Columns.Item("4").Cells.Item(i).Specific.String
            STMatrix.Columns.Item("2").Cells.Item(i).Specific.String = objMatrix.Columns.Item("5").Cells.Item(i).Specific.String
            STMatrix.Columns.Item("10").Cells.Item(i).Specific.String = objMatrix.Columns.Item("9").Cells.Item(i).Specific.String
        Next

    End Sub

    Private Sub DeliveryCopyFrom(ByVal FormUID As String)
        Dim objForm As SAPbouiCOM.Form
        objForm = objAddOn.objApplication.Forms.Item(FormUID)

    End Sub

    Private Sub APCreditMemoCopyFrom(ByVal FormUID As String)
        Dim objForm As SAPbouiCOM.Form
        objForm = objAddOn.objApplication.Forms.Item(FormUID)

    End Sub

    Private Sub GoodsIssueCopyFrom(ByVal FormUID As String)
        Dim objForm As SAPbouiCOM.Form
        objForm = objAddOn.objApplication.Forms.Item(FormUID)

    End Sub

    Sub RightClickEvent(ByRef EventInfo As SAPbouiCOM.ContextMenuInfo, ByRef BubbleEvent As Boolean)
        Try
            objForm = objAddOn.objApplication.Forms.Item(objForm.UniqueID)
            objMatrix = objForm.Items.Item("36").Specific
            If EventInfo.BeforeAction Then
                objForm.EnableMenu("1283", False)
                Select Case EventInfo.EventType

                    Case SAPbouiCOM.BoEventTypes.et_RIGHT_CLICK
                        Try
                            If EventInfo.ItemUID = "" Then Exit Try
                            If objForm.Items.Item(EventInfo.ItemUID).Specific.String <> "" Then
                                objForm.EnableMenu("772", True)  'Copy
                            ElseIf objForm.Items.Item(EventInfo.ItemUID).Specific.String = "" Then
                                objForm.EnableMenu("773", True)  'Paste
                            End If
                        Catch ex As Exception
                            objMatrix = objForm.Items.Item(EventInfo.ItemUID).Specific
                            If EventInfo.Row <= 0 Then If objForm.Mode <> SAPbouiCOM.BoFormMode.fm_FIND_MODE Then objForm.EnableMenu("772", True) : objForm.EnableMenu("784", True) : Exit Try
                            If objMatrix.Columns.Item(EventInfo.ColUID).Cells.Item(EventInfo.Row).Specific.String <> "" Then
                                objForm.EnableMenu("772", True)  'Copy
                            ElseIf objMatrix.Columns.Item(EventInfo.ColUID).Cells.Item(EventInfo.Row).Specific.String = "" Then
                                objForm.EnableMenu("773", True)  'Paste
                            End If
                        End Try
                        Select Case EventInfo.ItemUID
                            Case "36"
                                If (EventInfo.ColUID = "0") And objForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE And EventInfo.Row > 0 Then
                                    objForm.EnableMenu("1293", True)
                                Else
                                    objForm.EnableMenu("1293", False)
                                    objForm.EnableMenu("1283", False)
                                    objForm.EnableMenu("1284", False)
                                    'objForm.EnableMenu("1286", False)
                                End If
                            Case Else
                                objForm.EnableMenu("1293", False)
                                objForm.EnableMenu("1283", False)
                                objForm.EnableMenu("1284", False)
                                'objForm.EnableMenu("1286", False)
                        End Select
                End Select
            Else
                objForm.EnableMenu("772", False)
                objForm.EnableMenu("773", False)
                objForm.EnableMenu("784", False)
                objForm.EnableMenu("1293", False)
                objForm.EnableMenu("1283", False)
                objForm.EnableMenu("1284", False)
                objForm.EnableMenu("1286", False)
            End If
        Catch ex As Exception
            'objAddOn.objApplication.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        Finally
        End Try
    End Sub

    Sub DeleteRow(ByVal objMatrix As SAPbouiCOM.Matrix, ByVal TableName As String)
        Try
            Dim DBSource As SAPbouiCOM.DBDataSource
            'objMatrix = objform.Items.Item("20").Specific
            objMatrix.FlushToDataSource()
            DBSource = objForm.DataSources.DBDataSources.Item(TableName) '"@MIREJDET1"
            For i As Integer = 1 To objMatrix.VisualRowCount
                objMatrix.GetLineData(i)
                DBSource.Offset = i - 1
                DBSource.SetValue("LineId", DBSource.Offset, i)
                objMatrix.SetLineData(i)
                objMatrix.FlushToDataSource()
            Next
            DBSource.RemoveRecord(DBSource.Size - 1)
            objMatrix.LoadFromDataSource()

        Catch ex As Exception
            objAddOn.objApplication.StatusBar.SetText("DeleteRow  Method Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_None)
        Finally
        End Try
    End Sub

    Private Sub ChooseFromList_Filteration(ByVal FormUID As String, ByVal CFLID As String, ByVal ColAlias As String, ByVal ColValue As String, ByVal Query As String)
        Try
            objForm = objAddOn.objApplication.Forms.Item(FormUID)
            Dim oCFL As SAPbouiCOM.ChooseFromList = objForm.ChooseFromLists.Item(CFLID) '
            Dim oConds As SAPbouiCOM.Conditions
            Dim oCond As SAPbouiCOM.Condition
            Dim oEmptyConds As New SAPbouiCOM.Conditions
            Dim rsetCFL As SAPbobsCOM.Recordset = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            oCFL.SetConditions(oEmptyConds)
            oConds = oCFL.GetConditions()
            oCond = oConds.Add
            oCond.Alias = ColAlias ' "Active"
            oCond.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL
            oCond.CondVal = ColValue ' "Y"
            If Query <> "" Then
                rsetCFL.DoQuery(Query)
                If rsetCFL.RecordCount > 0 Then
                    oCond.Relationship = SAPbouiCOM.BoConditionRelationship.cr_AND
                    For i As Integer = 0 To rsetCFL.RecordCount - 1
                        If i = rsetCFL.RecordCount - 1 Then
                            oCond = oConds.Add
                            oCond.Alias = rsetCFL.Fields.Item(0).Name
                            oCond.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL
                            oCond.CondVal = rsetCFL.Fields.Item(0).Value
                        Else
                            oCond = oConds.Add
                            oCond.Alias = rsetCFL.Fields.Item(0).Name
                            oCond.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL
                            oCond.CondVal = rsetCFL.Fields.Item(0).Value
                            oCond.Relationship = SAPbouiCOM.BoConditionRelationship.cr_OR
                        End If
                        rsetCFL.MoveNext()
                    Next
                End If
            End If
            oCFL.SetConditions(oConds)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub ManageAttributes()
        Try
            objAddOn.objGenFunc.SetAutomanagedattribute_Editable(objForm, "20", True, True, False) 'Series
            objAddOn.objGenFunc.SetAutomanagedattribute_Editable(objForm, "25", True, True, False) 'Status
            objAddOn.objGenFunc.SetAutomanagedattribute_Editable(objForm, "21", True, True, False) 'Doc Num
            objAddOn.objGenFunc.SetAutomanagedattribute_Editable(objForm, "4", True, True, False) 'Location
            objAddOn.objGenFunc.SetAutomanagedattribute_Editable(objForm, "6", True, True, False) 'Security Name
            objAddOn.objGenFunc.SetAutomanagedattribute_Editable(objForm, "8", True, True, False) 'Type
            objAddOn.objGenFunc.SetAutomanagedattribute_Editable(objForm, "10", True, True, False) 'Party Id
            objAddOn.objGenFunc.SetAutomanagedattribute_Editable(objForm, "23", True, True, False) 'Date
            objAddOn.objGenFunc.SetAutomanagedattribute_Editable(objForm, "12", False, True, False) 'Party Name
            objAddOn.objGenFunc.SetAutomanagedattribute_Editable(objForm, "51", True, True, False) 'Branch
        Catch ex As Exception

        End Try
    End Sub

End Class




