Public Class clsGEToGRPO

    Public Const Formtype As String = "GEGRPO"
    Dim objForm As SAPbouiCOM.Form
    Dim objCombo As SAPbouiCOM.ComboBox
    Dim Header As SAPbouiCOM.DBDataSource
    Dim objRS As SAPbobsCOM.Recordset
    Dim strSQL As String
    Dim objMatrix As SAPbouiCOM.Matrix
    Dim objHeader As SAPbouiCOM.DBDataSource
    Dim objLine As SAPbouiCOM.DBDataSource

    Public Sub LoadScreen()
        If objAddOn.HANA Then
            GE_Inward_GRPO_Draft = objAddOn.objGenFunc.getSingleValue("Select ifnull(""U_GEIGRPD"",'Y')  from OADM")
        Else
            GE_Inward_GRPO_Draft = objAddOn.objGenFunc.getSingleValue("Select isnull(U_GEIGRPD,'Y') from OADM")
        End If

        If GE_Inward_GRPO_Draft = "N" Then
            If Not objAddOn.objApplication.Menus.Item("GT").SubMenus.Exists(clsGEToGRPO.Formtype.ToString) Then objAddOn.CreateMenu("", 3, "Gate Entry GRPO", SAPbouiCOM.BoMenuType.mt_STRING, clsGEToGRPO.Formtype, objAddOn.objApplication.Menus.Item("GT"))
        Else
            If objAddOn.objApplication.Menus.Item("GT").SubMenus.Exists(clsGEToGRPO.Formtype.ToString) Then objAddOn.objApplication.Menus.Item("GT").SubMenus.RemoveEx(clsGEToGRPO.Formtype.ToString)
        End If
        If GE_Inward_GRPO_Draft = "Y" Then objAddOn.objApplication.StatusBar.SetText("GE Inward GRPO Draft Setting is enabled.Disable the Setting...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error) : Exit Sub
        objForm = objAddOn.objUIXml.LoadScreenXML("GEToGRPO.xml", Mukesh.SBOLib.UIXML.enuResourceType.Embeded, Formtype)
        objMatrix = objForm.Items.Item("8").Specific
        objHeader = objForm.DataSources.DBDataSources.Item("@GEGRPO")
        objLine = objForm.DataSources.DBDataSources.Item("@GEGRPO1")
        InitForm(objForm.UniqueID)
        objForm.Visible = True
        objAddOn.objGenFunc.SetAutomanagedattribute_Editable(objForm, "2A", False, False, True)
        objAddOn.objGenFunc.SetAutomanagedattribute_Editable(objForm, "7", True, False, False)
        objAddOn.objGenFunc.SetAutomanagedattribute_Editable(objForm, "4", True, True, False)
        objAddOn.objGenFunc.SetAutomanagedattribute_Editable(objForm, "6", True, True, False)
        objAddOn.objGenFunc.SetAutomanagedattribute_Editable(objForm, "13", True, True, False)
        objAddOn.objGenFunc.SetAutomanagedattribute_Editable(objForm, "10", True, True, False)
        'objAddOn.objGenFunc.SetAutomanagedattribute_Editable(objForm, "20", True, True, False)
        objForm.Items.Item("18A").Left = objForm.Items.Item("17").Left '+ objForm.Items.Item("14").Width + 3
        objForm.Items.Item("18A").Top = objForm.Items.Item("14").Top


    End Sub

    Public Sub ItemEvent(ByVal FormUID As String, ByRef pval As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        Try
            objForm = objAddOn.objApplication.Forms.Item(FormUID)
            objMatrix = objForm.Items.Item("8").Specific
            If pval.BeforeAction Then
                Select Case pval.EventType

                    Case SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST
                        If pval.ItemUID = "4" Then
                            'ChooseFromList_Filteration(FormUID, "cflbpcod", "CardType", "S")
                            If objAddOn.HANA Then
                                ChooseFromList_Filteration_Query(FormUID, "cflbpcod", "", "", "Select distinct ""U_partyid"" ""CardCode"" from ""@MIGTIN"" Where ""U_partyid"" in (Select ""CardCode"" from OCRD Where ""CardType""='S')")
                            Else
                                ChooseFromList_Filteration_Query(FormUID, "cflbpcod", "", "", "Select distinct U_partyid CardCode from [@MIGTIN] Where U_partyid in (Select CardCode from OCRD Where CardType='S')")
                            End If

                        ElseIf pval.ItemUID = "6" Then
                            If objForm.Items.Item("4").Specific.String <> "" Then ChooseFromList_Filteration(FormUID, "cflge", "U_partyid", objForm.Items.Item("4").Specific.String, "Status", "O")
                        End If
                    Case SAPbouiCOM.BoEventTypes.et_KEY_DOWN
                        If pval.ItemUID = "8" And pval.ColUID = "7A" Then
                            If objForm.Items.Item("15").Specific.String <> "" Then BubbleEvent = False
                        End If

                    Case SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED
                        If pval.ItemUID = "17" Then
                            Dim oGEForm As SAPbouiCOM.Form
                            Try
                                objMatrix = objForm.Items.Item("8").Specific
                                objAddOn.objApplication.Menus.Item("MIGTIN").Activate()
                                oGEForm = objAddOn.objApplication.Forms.ActiveForm
                                oGEForm.Freeze(True)
                                oGEForm.Mode = SAPbouiCOM.BoFormMode.fm_FIND_MODE
                                oGEForm.Items.Item("57").Enabled = True
                                oGEForm.Items.Item("57").Specific.String = objForm.Items.Item("6").Specific.String
                                'oGEForm.Items.Item("21").Enabled = True
                                'strSQL = objAddOn.objGenFunc.getSingleValue("Select ""DocNum"" from ""@MIGTIN"" where ""DocEntry""=" & objForm.Items.Item("6").Specific.String & " ")
                                'oGEForm.Items.Item("21").Specific.String = strSQL 'objForm.Items.Item("6").Specific.String
                                'oGEForm.Items.Item("23").Specific.String = objMatrix.Columns.Item("3").Cells.Item(1).Specific.String
                                oGEForm.Items.Item("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular)
                                oGEForm.Mode = SAPbouiCOM.BoFormMode.fm_VIEW_MODE
                                Dim objGEmatrix As SAPbouiCOM.Matrix
                                objGEmatrix = oGEForm.Items.Item("36").Specific
                                objGEmatrix.AutoResizeColumns()
                                oGEForm.Freeze(False)
                            Catch ex As Exception
                                oGEForm.Freeze(False)
                                oGEForm = Nothing
                            Finally
                                GC.Collect()
                                GC.WaitForPendingFinalizers()
                            End Try
                        ElseIf pval.ItemUID = "18" Then 'Or pval.ItemUID = "18A"
                            If objForm.Items.Item("15").Specific.String = "" Then Exit Sub
                            Dim TEntry As String
                            If objAddOn.HANA Then
                                TEntry = objAddOn.objGenFunc.getSingleValue("Select 1 from ODRF where ""ObjType""='20' and ifnull(""DocStatus"",'')='O' and ""DocEntry""='" & objForm.Items.Item("15").Specific.String & "'")
                            Else
                                TEntry = objAddOn.objGenFunc.getSingleValue("Select 1 from ODRF where ObjType='20' and isnull(DocStatus,'')='O' and DocEntry='" & objForm.Items.Item("15").Specific.String & "'")
                            End If

                            Dim objlink As SAPbouiCOM.LinkedButton
                            objlink = objForm.Items.Item("18").Specific
                            objlink.LinkedObject = "-1"
                            If TEntry <> "" Then
                                objlink.LinkedObject = "112"
                                objForm.Items.Item("14").Specific.Caption = "GRPO Draft"
                            Else
                                objlink.LinkedObject = "20"
                                objForm.Items.Item("14").Specific.Caption = "GRPO DocEntry"
                            End If
                            'CreateMySimpleForm("ViewData", "Goods Receipt PO List", "OPDN", "PDN1", "20")
                        End If
                    Case SAPbouiCOM.BoEventTypes.et_CLICK
                        If objForm.Mode = SAPbouiCOM.BoFormMode.fm_FIND_MODE Then Exit Sub
                        If pval.ItemUID = "1" And (objForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE Or objForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE) Then
                            If objForm.Items.Item("6").Specific.String = "" Then BubbleEvent = False : objAddOn.objApplication.StatusBar.SetText("Gate Inward is missing...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error) : Exit Sub
                            If objMatrix.VisualRowCount = 0 Then BubbleEvent = False : objAddOn.objApplication.StatusBar.SetText("Row data is missing...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error) : Exit Sub
                                For Row As Integer = 1 To objMatrix.VisualRowCount
                                    If Val(objMatrix.Columns.Item("7A").Cells.Item(Row).Specific.String) = 0 Or objMatrix.Columns.Item("7A").Cells.Item(Row).Specific.String = "" Then
                                        objAddOn.objApplication.StatusBar.SetText("Qty is Missing... on Line: " & Row, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                                        BubbleEvent = False : Exit Sub
                                    End If
                                Next
                            End If
                            If objForm.Items.Item("15").Specific.String <> "" And pval.ItemUID = "2A" And objForm.Items.Item("2A").Enabled = True Then
                            If objAddOn.HANA Then
                                strSQL = objAddOn.objGenFunc.getSingleValue("Select T0.""DocStatus"" from ODRF T0 join OPDN T1 on T0.""DocEntry""=T1.""draftKey"" where T0.""ObjType""='20' and T1.""draftKey"" =" & objForm.Items.Item("15").Specific.String & " or T1.""DocEntry""=" & objForm.Items.Item("15").Specific.String & " ")
                            Else
                                strSQL = objAddOn.objGenFunc.getSingleValue("Select T0.DocStatus from ODRF T0 join OPDN T1 on T0.DocEntry=T1.draftKey where T0.ObjType='20' and T1.draftKey =" & objForm.Items.Item("15").Specific.String & " or T1.DocEntry=" & objForm.Items.Item("15").Specific.String & " ")
                            End If

                            If strSQL = "C" Then
                                objAddOn.objApplication.StatusBar.SetText("Already Created GRPO Document...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                            ElseIf strSQL = "O" Then
                                objAddOn.objApplication.StatusBar.SetText("Already Created GRPO Draft...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                            End If
                            objForm.Items.Item("2A").Enabled = False
                            BubbleEvent = False : Exit Sub
                        End If
                    Case SAPbouiCOM.BoEventTypes.et_VALIDATE
                        If pval.ItemUID = "8" And pval.ColUID = "7A" Then
                            If objForm.Items.Item("15").Specific.String <> "" Then Exit Sub
                            If objMatrix.Columns.Item("7A").Cells.Item(pval.Row).Specific.String > objMatrix.Columns.Item("7B").Cells.Item(pval.Row).Specific.String Then
                                objMatrix.Columns.Item("7A").Cells.Item(pval.Row).Specific.String = objMatrix.Columns.Item("7B").Cells.Item(pval.Row).Specific.String
                                objAddOn.objApplication.StatusBar.SetText("Excess Qty is not allowed... on Line: " & pval.Row, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                                BubbleEvent = False : Exit Sub
                            End If
                        End If
                End Select
            Else
                Select Case pval.EventType
                    Case SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED
                        If pval.ItemUID = "1" And pval.ActionSuccess And objForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE Then
                            InitForm(FormUID)
                        End If
                    Case SAPbouiCOM.BoEventTypes.et_FORM_RESIZE
                        objMatrix.AutoResizeColumns()
                    Case SAPbouiCOM.BoEventTypes.et_LOST_FOCUS

                    Case SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST
                        If pval.ItemUID = "4" Or pval.ItemUID = "6" Or (pval.ItemUID = "8" And pval.ColUID = "7C") Then
                            CFL(FormUID, pval)
                        End If

                    Case SAPbouiCOM.BoEventTypes.et_CLICK
                        If pval.ItemUID = "8" Then
                            objMatrix.SelectRow(pval.Row, True, False)
                        ElseIf pval.ItemUID = "7" And objForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE Then
                            GForm = objForm 'objAddOn.objApplication.Forms.ActiveForm
                            objAddOn.objItemDetails.LoadScreen(Formtype, objForm.TypeCount, "", objForm.Items.Item("4").Specific.string, "", objHeader.GetValue("U_GEEntry", 0))
                        ElseIf pval.ItemUID = "2A" Then
                            If objForm.Mode <> SAPbouiCOM.BoFormMode.fm_ADD_MODE Or objForm.Mode <> SAPbouiCOM.BoFormMode.fm_FIND_MODE Then
                                If Create_GoodsReceipt_PO(FormUID, objForm.Items.Item("6").Specific.String) Then
                                    objAddOn.objApplication.StatusBar.SetText("GRPO Draft Created Successfully...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
                                End If
                            End If
                        End If
                    Case SAPbouiCOM.BoEventTypes.et_KEY_DOWN
                        If pval.ItemUID = "8" Then
                            Dim ColID As Integer = objMatrix.GetCellFocus().ColumnIndex
                            If pval.CharPressed = 38 And pval.Modifiers = SAPbouiCOM.BoModifiersEnum.mt_None Then  'up
                                objMatrix.SetCellFocus(pval.Row - 1, ColID)
                                objMatrix.SelectRow(pval.Row - 1, True, False)
                            ElseIf pval.CharPressed = 40 And pval.Modifiers = SAPbouiCOM.BoModifiersEnum.mt_None Then 'down
                                objMatrix.SetCellFocus(pval.Row + 1, ColID)
                                objMatrix.SelectRow(pval.Row + 1, True, False)
                            End If
                        End If
                    Case SAPbouiCOM.BoEventTypes.et_COMBO_SELECT
                        If pval.ItemUID = "10" Then
                            If objForm.Mode <> SAPbouiCOM.BoFormMode.fm_ADD_MODE Then Exit Sub
                            objHeader.SetValue("DocNum", 0, objAddOn.objGenFunc.GetDocNum("GEGRP", CInt(objForm.Items.Item("10").Specific.Selected.Value)))
                        End If

                End Select

            End If
        Catch ex As Exception

        End Try
    End Sub

    Public Sub FormDataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean)
        Try
            objForm = objAddOn.objApplication.Forms.Item(BusinessObjectInfo.FormUID)
            objMatrix = objForm.Items.Item("8").Specific
            If BusinessObjectInfo.BeforeAction Then
                Try
                    If (BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD Or BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE) Then

                    End If
                Catch ex As Exception
                    objAddOn.objApplication.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
                End Try
            Else
                Select Case BusinessObjectInfo.EventType
                    Case SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD
                        If BusinessObjectInfo.ActionSuccess Then
                            If objAddOn.HANA Then
                                strSQL = "Update ""@MIGTIN"" Set ""U_Prostat""='2' where ""DocEntry""=" & objForm.Items.Item("6").Specific.String & ""
                            Else
                                strSQL = "Update [@MIGTIN] Set U_Prostat='2' where DocEntry=" & objForm.Items.Item("6").Specific.String & ""
                            End If
                            objRS = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            objRS.DoQuery(strSQL)
                        End If
                    Case SAPbouiCOM.BoEventTypes.et_FORM_DATA_LOAD
                        objMatrix.AutoResizeColumns()
                        If objForm.Items.Item("15").Specific.String <> "" Then objForm.Items.Item("2A").Enabled = False Else objForm.Items.Item("2A").Enabled = True
                End Select

            End If
        Catch ex As Exception

        End Try

    End Sub

    Public Sub MenuEvent(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean)
        Try
            objMatrix = objForm.Items.Item("8").Specific
            Select Case pVal.MenuUID
                Case "1284", "1286"
                    If pVal.BeforeAction = True Then
                        If pVal.MenuUID = "1284" Then 'Cancel
                            If objAddOn.objApplication.MessageBox("Cancelling of an entry cannot be reversed. Do you want to continue?", 2, "Yes", "No") <> 1 Then BubbleEvent = False : Exit Sub
                        ElseIf pVal.MenuUID = "1286" Then 'Close
                            If objAddOn.objApplication.MessageBox("Closing of an entry cannot be reversed. Do you want to continue?", 2, "Yes", "No") <> 1 Then BubbleEvent = False : Exit Sub
                        End If
                        objRS = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                        If objAddOn.HANA Then
                            strSQL = "Update ""@MIGTIN"" Set ""U_GEGR""='',""U_Prostat""='6' where ""DocEntry""=" & objForm.Items.Item("6").Specific.String & ""
                        Else
                            strSQL = "Update [@MIGTIN] Set U_GEGR='',U_Prostat='6' where DocEntry=" & objForm.Items.Item("6").Specific.String & ""
                        End If

                        objRS.DoQuery(strSQL)
                    End If

                Case "1282"
                    If pVal.BeforeAction = False Then InitForm(objAddOn.objApplication.Forms.ActiveForm.UniqueID)
                    'objMatrix.Item.Enabled = True
                    'Case "1289"
                    '    If pVal.BeforeAction = False Then Me.UpdateMode()
                Case "1293"  'delete Row

                Case "1281"
                    objMatrix.Item.Enabled = False
                    objForm.Items.Item("11").Enabled = True
                    objForm.Items.Item("15").Enabled = True
                    objForm.Items.Item("10").Enabled = True
                    objForm.Items.Item("20").Enabled = True
                    objForm.Items.Item("11A").Enabled = True
            End Select
        Catch ex As Exception
            'objAddOn.objApplication.StatusBar.SetText("Menu Event Failed:" & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
        Finally
        End Try
    End Sub

    Sub RightClickEvent(ByRef EventInfo As SAPbouiCOM.ContextMenuInfo, ByRef BubbleEvent As Boolean)
        Try
            objForm = objAddOn.objApplication.Forms.Item(objForm.UniqueID)
            objMatrix = objForm.Items.Item("8").Specific
            If EventInfo.BeforeAction Then
                objForm.EnableMenu("1283", False)
                objForm.EnableMenu("1285", False)
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
                            Case ""
                                If objForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE Or objForm.Mode = SAPbouiCOM.BoFormMode.fm_FIND_MODE Then Exit Sub
                                If objForm.Items.Item("15").Specific.String = "" And objForm.Items.Item("24").Specific.Selected.Value = "O" Then
                                    objForm.EnableMenu("1284", True) 'Cancel
                                    objForm.EnableMenu("1286", True) 'Close
                                Else
                                    objForm.EnableMenu("1284", False) 'Cancel
                                    objForm.EnableMenu("1286", False) 'Close
                                End If
                            Case Else
                                objForm.EnableMenu("1284", False) 'Cancel
                                objForm.EnableMenu("1286", False) 'Close
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

    Public Sub InitForm(ByVal FormUID As String)
        LoadSeries(FormUID)
        objForm = objAddOn.objApplication.Forms.Item(FormUID)
        objMatrix = objForm.Items.Item("8").Specific
        'objMatrix.Columns.Item("9").ColumnSetting.SumType = SAPbouiCOM.BoColumnSumType.bst_Auto
        'objMatrix.Columns.Item("12").ColumnSetting.SumType = SAPbouiCOM.BoColumnSumType.bst_Auto
        objMatrix.AutoResizeColumns()
    End Sub

    Public Sub LoadSeries(ByVal FormUID As String)
        Dim StrDocNum
        objForm = objAddOn.objApplication.Forms.Item(FormUID)
        objForm.Items.Item("20").Specific.String = "Created By " & objAddOn.objCompany.UserName & " on " & Now.ToString("dd/MMM/yyyy HH:mm:ss")

        '----------------Load series --------------
        objCombo = objForm.Items.Item("10").Specific
        objCombo.ValidValues.LoadSeries("GEGRP", SAPbouiCOM.BoSeriesMode.sf_Add)
        If objCombo.ValidValues.Count > 0 Then objCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index)

        Try
            StrDocNum = objForm.BusinessObject.GetNextSerialNumber(Trim(objForm.Items.Item("10").Specific.Selected.value), objForm.BusinessObject.Type)
        Catch ex As Exception
            objAddOn.objApplication.MessageBox("To generate this document, first define the numbering series in the Administration module")
            Exit Sub
        End Try
        objHeader = objForm.DataSources.DBDataSources.Item("@GEGRPO")
        objHeader.SetValue("DocNum", 0, StrDocNum)
        'objHeader.SetValue("DocNum", 0, objAddOn.objGenFunc.GetDocNum("GEGRP", CInt(objForm.Items.Item("10").Specific.Selected.value)))
        objForm.Items.Item("13").Specific.String = "A" ' current date


    End Sub

    Private Sub ChooseFromList_Filteration(ByVal FormUID As String, ByVal CFLID As String, ByVal ColAlias As String, ByVal ColValue As String, ByVal ColAlias1 As String, ByVal ColValue1 As String)
        Try
            objForm = objAddOn.objApplication.Forms.Item(FormUID)
            Dim oCFL As SAPbouiCOM.ChooseFromList = objForm.ChooseFromLists.Item(CFLID)
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

            If ColAlias1 <> "" And ColValue1 <> "" Then
                oCond.Relationship = SAPbouiCOM.BoConditionRelationship.cr_AND
                oCond = oConds.Add
                oCond.Alias = ColAlias1
                oCond.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL
                oCond.CondVal = ColValue1
            End If
            oCFL.SetConditions(oConds)

        Catch ex As Exception

        End Try
    End Sub

    Private Sub ChooseFromList_Filteration_Query(ByVal FormUID As String, ByVal CFLID As String, ByVal ColAlias As String, ByVal ColValue As String, ByVal Query As String)
        Try
            objForm = objAddOn.objApplication.Forms.Item(FormUID)
            Dim oCFL As SAPbouiCOM.ChooseFromList = objForm.ChooseFromLists.Item(CFLID)
            Dim oConds As SAPbouiCOM.Conditions
            Dim oCond As SAPbouiCOM.Condition
            Dim oEmptyConds As New SAPbouiCOM.Conditions
            Dim rsetCFL As SAPbobsCOM.Recordset = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            rsetCFL.DoQuery(Query)
            oCFL.SetConditions(oEmptyConds)
            oConds = oCFL.GetConditions()

            If ColAlias <> "" And ColValue <> "" Then
                oCond = oConds.Add
                oCond.Alias = ColAlias
                oCond.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL
                oCond.CondVal = ColValue
            End If


            If rsetCFL.RecordCount > 0 Then
                If oConds.Count > 0 Then oCond.Relationship = SAPbouiCOM.BoConditionRelationship.cr_AND
                For i As Integer = 1 To rsetCFL.RecordCount
                    If i = rsetCFL.RecordCount Then
                        oCond = oConds.Add()
                        oCond.Alias = rsetCFL.Fields.Item(0).Name
                        oCond.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL
                        oCond.CondVal = Trim(rsetCFL.Fields.Item(0).Value)
                    Else
                        oCond = oConds.Add()
                        oCond.Alias = rsetCFL.Fields.Item(0).Name
                        oCond.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL
                        oCond.CondVal = Trim(rsetCFL.Fields.Item(0).Value)
                        oCond.Relationship = SAPbouiCOM.BoConditionRelationship.cr_OR
                    End If
                    rsetCFL.MoveNext()
                Next
            Else

            End If
            oCFL.SetConditions(oConds)

        Catch ex As Exception

        End Try
    End Sub

    Private Sub CFL(ByVal FormUID As String, ByVal pval As SAPbouiCOM.ItemEvent)
        Try
            Dim objCFLEvent As SAPbouiCOM.ChooseFromListEvent
            Dim objDataTable As SAPbouiCOM.DataTable
            objCFLEvent = pval
            objDataTable = objCFLEvent.SelectedObjects
            objForm = objAddOn.objApplication.Forms.Item(FormUID)
            objMatrix = objForm.Items.Item("8").Specific
            Select Case objCFLEvent.ChooseFromListUID
                Case "cflbpcod"
                    Try
                        If objDataTable Is Nothing Then
                        Else
                            objForm.Items.Item("4").Specific.String = objDataTable.GetValue("CardCode", 0)
                        End If
                    Catch ex As Exception
                        objForm.Items.Item("4").Specific.String = objDataTable.GetValue("CardCode", 0)
                    End Try
                Case "cflge"
                    Try
                        If objDataTable Is Nothing Then
                        Else
                            objForm.Items.Item("6").Specific.String = objDataTable.GetValue("DocEntry", 0)
                        End If
                    Catch ex As Exception
                        objForm.Items.Item("6").Specific.String = objDataTable.GetValue("DocEntry", 0)
                    End Try
                Case "cflwhse"
                    Try
                        If objDataTable Is Nothing Then
                        Else
                            objMatrix.Columns.Item("7C").Cells.Item(pval.Row).Specific.String = objDataTable.GetValue("WhsCode", 0)
                        End If
                    Catch ex As Exception
                        objMatrix.Columns.Item("7C").Cells.Item(pval.Row).Specific.String = objDataTable.GetValue("WhsCode", 0)
                    End Try
            End Select
        Catch ex As Exception
            'MsgBox("Error : " + ex.Message + vbCrLf + "Position : " + ex.StackTrace, MsgBoxStyle.Critical)
        End Try

    End Sub

    Private Function Create_GoodsReceipt_PO(ByVal FormUID As String, ByVal GEEntry As String)
        Try
            If objForm.Items.Item("2A").Enabled = False Then Return False

            Dim objedit As SAPbouiCOM.EditText
            Dim objGRPO As SAPbobsCOM.Documents
            Dim DocEntry, strQuery As String
            Dim Lineflag As Boolean = False
            Dim Row As Integer = 1
            objForm = objAddOn.objApplication.Forms.Item(FormUID)
            If objForm.Items.Item("15").Specific.String <> "" Then Return True

            objGRPO = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDrafts)

            objRS = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            objAddOn.objApplication.StatusBar.SetText("Creating Goods Receipt PO draft. Please wait...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
            objedit = objForm.Items.Item("13").Specific
            Dim DocDate As Date = Date.ParseExact(objedit.Value, "yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo)
            If objAddOn.HANA Then
                strQuery = objAddOn.objGenFunc.getSingleValue("Select Distinct 1 as ""Status"" from ""@GEGRPO1"" B join POR1 A on B.""U_PoEntry""=A.""DocEntry"" and B.""U_ItemCode""=A.""ItemCode"" and B.""U_PoLine""=A.""LineNum"" where  B.""DocEntry""=" & objHeader.GetValue("DocEntry", 0) & " and A.""LineStatus""='C' ")
            Else
                strQuery = objAddOn.objGenFunc.getSingleValue("Select Distinct 1 as Status from [@GEGRPO1] B join POR1 A on B.U_PoEntry=A.DocEntry and B.U_ItemCode=A.ItemCode and B.U_PoLine=A.LineNum where  B.DocEntry=" & objHeader.GetValue("DocEntry", 0) & " and A.LineStatus='C' ")
            End If

            If strQuery = "1" Then objAddOn.objApplication.StatusBar.SetText("PO Status Closed for this Transaction...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error) : Return False
            'strQuery = "Select B.* from ""@GEGRPO"" A join ""@GEGRPO1"" B on A.""DocEntry""=B.""DocEntry"" where A.""U_GRPOEntry"" is null and A.""U_GEEntry""='" & GEEntry & "'"

            'objRS.DoQuery(strQuery)
            'If objRS.RecordCount = 0 Then objAddOn.objApplication.StatusBar.SetText("No Records Found...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error) : Return False
            If Not objAddOn.objCompany.InTransaction Then objAddOn.objCompany.StartTransaction()

            objGRPO.DocDate = DocDate
            objGRPO.JournalMemo = "Auto-Gen-> " & Now.ToString
            objGRPO.DocObjectCode = SAPbobsCOM.BoObjectTypes.oPurchaseDeliveryNotes
            objGRPO.UserFields.Fields.Item("U_GEGR").Value = objHeader.GetValue("DocEntry", 0)
            objGRPO.UserFields.Fields.Item("U_gever").Value = GEEntry
            'objGRPO.UserFields.Fields.Item("U_GEEntry").Value = GEEntry
            strQuery = "Select ""BPLId"" from OBPL where ""Disabled""='N' and ""MainBPL""='Y'" 'Branch
            strQuery = objAddOn.objGenFunc.getSingleValue(strQuery)
            If strQuery <> "" Then objGRPO.BPL_IDAssignedToInvoice = strQuery
            If objMatrix.VisualRowCount > 0 Then
                If objGRPO.CardCode = "" Then objGRPO.CardCode = Trim(objForm.Items.Item("4").Specific.String)
                For i As Integer = 1 To objMatrix.VisualRowCount
                    objGRPO.Lines.ItemCode = Trim(objMatrix.Columns.Item("4").Cells.Item(i).Specific.String)
                    objGRPO.Lines.Quantity = CDbl(objMatrix.Columns.Item("7A").Cells.Item(i).Specific.String) ' CDbl(Matrix0.Columns.Item("grnqty").Cells.Item(i).Specific.String) ' CDbl(objRs.Fields.Item("GRN Qty").Value.ToString) 
                    'objGRPO.Lines.AccountCode = Trim(objRS.Fields.Item("AcctCode").Value.ToString)
                    'objGRPO.Lines.TaxCode = Trim(objRS.Fields.Item("TaxCode").Value.ToString)
                    objGRPO.Lines.BaseType = 22
                    objGRPO.Lines.BaseEntry = CInt(objMatrix.Columns.Item("8").Cells.Item(i).Specific.String) ' CInt(objRs.Fields.Item("PO Entry").Value.ToString)
                    objGRPO.Lines.BaseLine = CInt(objMatrix.Columns.Item("9").Cells.Item(i).Specific.String)
                    'objGRPO.Lines.UnitPrice = Trim(objRS.Fields.Item("Price").Value.ToString)
                    objGRPO.Lines.WarehouseCode = objMatrix.Columns.Item("7C").Cells.Item(i).Specific.String
                    objGRPO.Lines.UserFields.Fields.Item("U_GateQty").Value = CDbl(objMatrix.Columns.Item("7A").Cells.Item(i).Specific.String)
                    objGRPO.Lines.UserFields.Fields.Item("U_geentry").Value = GEEntry ' objMatrix.Columns.Item("7A").Cells.Item(i).Specific.String
                    objGRPO.Lines.Add()
                Next
            End If
            'If objRS.RecordCount > 0 Then
            '    If objGRPO.CardCode = "" Then objGRPO.CardCode = Trim(objForm.Items.Item("4").Specific.String)
            '    For i As Integer = 0 To objRS.RecordCount - 1
            '        objGRPO.Lines.ItemCode = Trim(objRS.Fields.Item("U_ItemCode").Value.ToString)
            '        objGRPO.Lines.Quantity = CDbl(objRS.Fields.Item("U_Qty").Value) ' CDbl(Matrix0.Columns.Item("grnqty").Cells.Item(i).Specific.String) ' CDbl(objRs.Fields.Item("GRN Qty").Value.ToString) 
            '        'objGRPO.Lines.AccountCode = Trim(objRS.Fields.Item("AcctCode").Value.ToString)
            '        'objGRPO.Lines.TaxCode = Trim(objRS.Fields.Item("TaxCode").Value.ToString)
            '        objGRPO.Lines.BaseType = 22
            '        objGRPO.Lines.BaseEntry = CInt(objRS.Fields.Item("U_PoEntry").Value.ToString) ' CInt(objRs.Fields.Item("PO Entry").Value.ToString)
            '        objGRPO.Lines.BaseLine = CInt(objRS.Fields.Item("U_PoLine").Value.ToString)
            '        'objGRPO.Lines.UnitPrice = Trim(objRS.Fields.Item("Price").Value.ToString)
            '        'objGRPO.Lines.WarehouseCode = Trim(objRS.Fields.Item("WhsCode").Value.ToString)
            '        objGRPO.Lines.UserFields.Fields.Item("U_GateQty").Value = CDbl(objRS.Fields.Item("U_Qty").Value)
            '        objGRPO.Lines.Add()
            '        objRS.MoveNext()
            '    Next
            'End If

            If objGRPO.Add() <> 0 Then
                If objAddOn.objCompany.InTransaction Then objAddOn.objCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                objAddOn.objApplication.SetStatusBarMessage("GRPO: " & objAddOn.objCompany.GetLastErrorDescription & "-" & objAddOn.objCompany.GetLastErrorCode, SAPbouiCOM.BoMessageTime.bmt_Medium, True)
                objAddOn.objApplication.MessageBox("GRPO: " & objAddOn.objCompany.GetLastErrorDescription & "-" & objAddOn.objCompany.GetLastErrorCode,, "OK")
                Return False
            Else
                If objAddOn.objCompany.InTransaction Then objAddOn.objCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                DocEntry = objAddOn.objCompany.GetNewObjectKey()
                objForm.Items.Item("15").Specific.String = DocEntry
                If objAddOn.HANA Then
                    strQuery = "Update ""@MIGTIN"" Set ""U_GEGR""=" & objHeader.GetValue("DocEntry", 0) & ",""U_Prostat""='3',""U_trgtentry""=case when ""U_trgtentry"" is null then " & DocEntry & " else ""U_trgtentry""||','||" & DocEntry & " End   where ""DocEntry""=" & GEEntry & ""
                Else
                    strQuery = "Update [@MIGTIN] Set U_GEGR=" & objHeader.GetValue("DocEntry", 0) & ",U_Prostat='3',U_trgtentry=case when U_trgtentry is null then " & DocEntry & " else U_trgtentry + ',' + " & DocEntry & " End   where DocEntry=" & GEEntry & ""
                End If
                objRS = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                objRS.DoQuery(strQuery)
                If objForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE Then objForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE
                If objForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE Then
                    objForm.Items.Item("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular)
                End If
                'EditText5.Value = DocEntry

                objAddOn.objApplication.StatusBar.SetText("Draft GRPO Created Successfully...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
                Return True
            End If
            System.Runtime.InteropServices.Marshal.ReleaseComObject(objGRPO)
            GC.Collect()
        Catch ex As Exception

        End Try
    End Function

    Private Sub CreateMySimpleForm(ByVal FormID As String, ByVal FormTitle As String, ByVal Header As String, ByVal Line As String, ByVal LinkedID As String)
        Dim oCreationParams As SAPbouiCOM.FormCreationParams
        Dim objTempForm As SAPbouiCOM.Form
        Dim objrs As SAPbobsCOM.Recordset
        Try
            objAddOn.objApplication.Forms.Item(FormID).Visible = True
        Catch ex As Exception
            oCreationParams = objAddOn.objApplication.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_FormCreationParams)
            oCreationParams.BorderStyle = SAPbouiCOM.BoFormBorderStyle.fbs_Sizable
            oCreationParams.UniqueID = FormID
            objTempForm = objAddOn.objApplication.Forms.AddEx(oCreationParams)
            objTempForm.Title = FormTitle
            objTempForm.Left = 400
            objTempForm.Top = 100
            objTempForm.ClientHeight = 200 '335
            objTempForm.ClientWidth = 500
            objTempForm.Left = objForm.Left + 100
            objTempForm.Top = objForm.Top + 100
            objTempForm = objAddOn.objApplication.Forms.Item(FormID)
            Dim oitm As SAPbouiCOM.Item

            Dim oGrid As SAPbouiCOM.Grid
            oitm = objTempForm.Items.Add("Grid", SAPbouiCOM.BoFormItemTypes.it_GRID)
            oitm.Top = 30
            oitm.Left = 2
            oitm.Width = 490
            oitm.Height = 120
            oGrid = objTempForm.Items.Item("Grid").Specific
            objTempForm.DataSources.DataTables.Add("DataTable")
            oitm = objTempForm.Items.Add("2", SAPbouiCOM.BoFormItemTypes.it_BUTTON)
            oitm.Top = objTempForm.Items.Item("Grid").Top + objTempForm.Items.Item("Grid").Height + 10
            oitm.Left = 2
            Dim str_sql As String = ""
            If objForm.Items.Item("15").Specific.String = "" Then objAddOn.objApplication.StatusBar.SetText("No Entries Found...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning) : Exit Sub
            If objAddOn.HANA Then
                str_sql = "select T0.""DocEntry"",T0.""DocNum"",T0.""DocDate"" from OPDN T0 where T0.""U_gever""='" & objMatrix.Columns.Item("2").Cells.Item(1).Specific.String & "' and ""CANCELED""='N'"
            Else
                str_sql = "select T0.DocEntry,T0.DocNum,T0.DocDate from OPDN T0 where T0.U_gever='" & objMatrix.Columns.Item("2").Cells.Item(1).Specific.String & "' and CANCELED='N'"
            End If

            objrs = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            objrs.DoQuery(str_sql)
            If objrs.RecordCount = 0 Then objAddOn.objApplication.StatusBar.SetText("No Entries Found...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning) : objrs = Nothing : Exit Sub
            Dim objDT As SAPbouiCOM.DataTable
            objDT = objTempForm.DataSources.DataTables.Item("DataTable")
            objDT.Clear()
            objDT.ExecuteQuery(str_sql)
            objTempForm.DataSources.DataTables.Item("DataTable").ExecuteQuery(str_sql)

            oGrid.DataTable = objTempForm.DataSources.DataTables.Item("DataTable")

            For i As Integer = 0 To oGrid.Columns.Count - 1
                oGrid.Columns.Item(i).TitleObject.Sortable = True
                oGrid.Columns.Item(i).Editable = False
            Next

            oGrid.Columns.Item(0).Type = SAPbouiCOM.BoGridColumnType.gct_EditText
            Dim col As SAPbouiCOM.EditTextColumn
            col = oGrid.Columns.Item(0)
            col.LinkedObjectType = LinkedID
            objTempForm.Visible = True
            objTempForm.Update()
            'bModal = True
            'FormName = "ViewD"
        End Try
    End Sub

End Class

