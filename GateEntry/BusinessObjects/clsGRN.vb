Public Class clsGRN
    Public Const formtype As String = "143"
    Dim objForm, objUDFFormID As SAPbouiCOM.Form
    Dim strSQL As String
    Dim objRS As SAPbobsCOM.Recordset
    Dim GEDocEntry As String
    Dim objMatrix As SAPbouiCOM.Matrix
    Dim objHeader As SAPbouiCOM.DBDataSource
    Dim objLine As SAPbouiCOM.DBDataSource

    Public Sub ItemEvent(ByVal FormUID As String, ByRef pval As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        Try
            If pval.BeforeAction Then
                objForm = objAddOn.objApplication.Forms.Item(FormUID)
                objMatrix = objForm.Items.Item("38").Specific
                Select Case pval.EventType

                    Case SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED
                        If pval.ItemUID = "1" And (objForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE Or objForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE) Then
                            If objForm.Items.Item("3").Specific.Selected.Value = "S" Then Exit Sub
                            'objUDFFormID = objAddOn.objApplication.Forms.Item(objForm.UDFFormUID)
                            'If objUDFFormID.Items.Item("U_GEGR").Specific.String <> "" Or objUDFFormID.Items.Item("U_GEEntry").Specific.String <> "" Then Exit Sub
                            If Not GEVerfication(FormUID) Then
                                BubbleEvent = False
                                Exit Sub
                            End If
                            If GE_Inward_GRPO_Draft = "N" Then
                                objUDFFormID = objAddOn.objApplication.Forms.Item(objForm.UDFFormUID)
                                If objUDFFormID.Items.Item("U_GEGR").Specific.String = "" Then
                                    BubbleEvent = False
                                    objAddOn.objApplication.StatusBar.SetText("GE To GRPO details not found...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                                    Exit Sub
                                End If
                            End If
                            If objMatrix.VisualRowCount = 0 Then Exit Sub
                            For i As Integer = 1 To objMatrix.VisualRowCount
                                If objMatrix.Columns.Item("1").Cells.Item(i).Specific.String <> "" Then
                                    If objMatrix.Columns.Item("U_GateQty").Cells.Item(i).Specific.String = 0 Then
                                        objAddOn.objApplication.StatusBar.SetText("Please remove the Gate Entry is not happened for the item. on Line: " & i, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                                        objMatrix.Columns.Item("1").Cells.Item(i).Click()
                                        BubbleEvent = False
                                        Exit Sub
                                    ElseIf CDbl(objMatrix.Columns.Item("U_GateQty").Cells.Item(i).Specific.String) <> CDbl(objMatrix.Columns.Item("11").Cells.Item(i).Specific.String) Then
                                        'objAddOn.objApplication.StatusBar.SetText("Excess Gate Entry for the item. on Line: " & i, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                                        objAddOn.objApplication.StatusBar.SetText("Gate Entry Qty is not matching for the item. on Line: " & i, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                                        objMatrix.Columns.Item("1").Cells.Item(i).Click()
                                        BubbleEvent = False
                                        Exit Sub
                                    ElseIf objMatrix.Columns.Item("U_geentry").Cells.Item(i).Specific.String = "" Then
                                        objAddOn.objApplication.StatusBar.SetText("Gate Entry is missing for the item. on Line: " & i, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                                        objMatrix.Columns.Item("1").Cells.Item(i).Click()
                                        BubbleEvent = False
                                        Exit Sub
                                    End If
                                End If
                            Next
                        ElseIf pval.ItemUID = "gelink" Then
                            Dim oGEForm As SAPbouiCOM.Form
                            Try
                                BubbleEvent = False
                                objAddOn.objApplication.Menus.Item("MIGTIN").Activate()
                                oGEForm = objAddOn.objApplication.Forms.ActiveForm 'objAddOn.objApplication.Forms.GetForm("MIPLQC", 1)
                                oGEForm.Freeze(True)
                                oGEForm.Mode = SAPbouiCOM.BoFormMode.fm_FIND_MODE
                                oGEForm.Items.Item("57").Enabled = True
                                oGEForm.Items.Item("57").Specific.String = objForm.Items.Item("TVer").Specific.String 'objMatrix.Columns.Item("1B").Cells.Item(pval.Row).Specific.String
                                oGEForm.Items.Item("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular)
                                oGEForm.Mode = SAPbouiCOM.BoFormMode.fm_VIEW_MODE
                                Dim objmatrix As SAPbouiCOM.Matrix
                                objmatrix = oGEForm.Items.Item("36").Specific
                                objmatrix.AutoResizeColumns()
                                oGEForm.Freeze(False)
                            Catch ex As Exception
                                oGEForm.Freeze(False)
                                oGEForm = Nothing
                            Finally
                                GC.Collect()
                                GC.WaitForPendingFinalizers()
                            End Try

                        End If

                    Case SAPbouiCOM.BoEventTypes.et_KEY_DOWN
                        'If pval.ItemUID = "TVer" Then
                        '    BubbleEvent = False
                        'End If
                        If pval.CharPressed = 9 Or objForm.Mode = SAPbouiCOM.BoFormMode.fm_FIND_MODE Then Exit Sub
                        If pval.ItemUID = "38" And (pval.ColUID = "U_GateQty" Or pval.ColUID = "U_DiffQty" Or pval.ColUID = "U_geentry") And pval.InnerEvent = False Then
                            BubbleEvent = False
                        ElseIf pval.ItemUID = "TVer" And pval.CharPressed <> 9 Then
                            BubbleEvent = False
                        End If
                End Select
            Else
                Select Case pval.EventType
                    Case SAPbouiCOM.BoEventTypes.et_FORM_LOAD
                        Create_CustomizedFields(FormUID)
                        objHeader = objForm.DataSources.DBDataSources.Item("OPDN")
                        objLine = objForm.DataSources.DBDataSources.Item("PDN1")
                    Case SAPbouiCOM.BoEventTypes.et_CLICK
                        If pval.ItemUID = "Verify" And objForm.Mode <> SAPbouiCOM.BoFormMode.fm_FIND_MODE Then
                            ''Verify(FormUID)
                            'For i As Integer = 1 To objMatrix.VisualRowCount
                            '    If objMatrix.Columns.Item("1").Cells.Item(i).Specific.String <> "" Then
                            '        'Dim GQty As Double = IIf(objMatrix.Columns.Item("U_GateQty").Cells.Item(i).Specific.String = "", 0, CDbl(objMatrix.Columns.Item("U_GateQty").Cells.Item(i).Specific.String))
                            '        objMatrix.Columns.Item("U_DiffQty").Cells.Item(i).Specific.String = CDbl(objMatrix.Columns.Item("U_GateQty").Cells.Item(i).Specific.String) - CDbl(objMatrix.Columns.Item("11").Cells.Item(i).Specific.String)
                            '    End If
                            'Next
                        End If

                    Case SAPbouiCOM.BoEventTypes.et_KEY_DOWN
                        If pval.ItemUID = "TVer" And pval.CharPressed = 9 And pval.Modifiers = SAPbouiCOM.BoModifiersEnum.mt_None Then
                            If objMatrix.Columns.Item("1").Cells.Item(1).Specific.String = "" Then objAddOn.objApplication.StatusBar.SetText("Line Data is Missing...", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error) : Exit Sub
                            GForm = objAddOn.objApplication.Forms.ActiveForm
                            If GForm.Items.Item("TVer").Specific.String = "" Then objAddOn.objItemDetails.LoadScreen(formtype, GForm.TypeCount, "", GForm.Items.Item("4").Specific.String, "", "")
                        End If

                End Select
            End If
        Catch ex As Exception

        End Try
    End Sub

    Public Sub FormDataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean)
        Try
            objForm = objAddOn.objApplication.Forms.Item(BusinessObjectInfo.FormUID)
            If BusinessObjectInfo.BeforeAction Then
                Try
                    If (BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD Or BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE) Then
                        If GEVerfication(BusinessObjectInfo.FormUID) Then
                        Else
                            BubbleEvent = False
                        End If
                    End If
                Catch ex As Exception
                    'objAddOn.objApplication.StatusBar.SetText(ex.ToString, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
                End Try
            Else
                Select Case BusinessObjectInfo.EventType
                    Case SAPbouiCOM.BoEventTypes.et_FORM_DATA_LOAD
                        objForm.Items.Item("TVer").Enabled = False

                    Case SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD ', SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE
                        If BusinessObjectInfo.ActionSuccess = True Then
                            objUDFFormID = objAddOn.objApplication.Forms.Item(objForm.UDFFormUID)
                            If objUDFFormID.Items.Item("U_gever").Specific.String = "" Then Exit Sub
                            Dim GRPOEntry As String = objForm.DataSources.DBDataSources.Item("OPDN").GetValue("DocEntry", 0)
                            If objForm.Title = "Goods Receipt PO - Cancellation" Then
                                objRS = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                strSQL = "Update ""@MIGTIN"" Set ""U_trgtentry""='',""U_Prostat""='5'  where ""DocEntry""='" & objUDFFormID.Items.Item("U_gever").Specific.String & "'"
                                objRS.DoQuery(strSQL)
                                strSQL = "Update OPDN Set ""U_gever""='',""U_GEGR""='' where ""DocEntry""='" & GRPOEntry & "'"
                                objRS.DoQuery(strSQL)
                                strSQL = "Update PDN1 Set ""U_GateQty""=0,""U_DiffQty""=0 where ""DocEntry""='" & GRPOEntry & "'"
                                objRS.DoQuery(strSQL)
                                If GE_Inward_GRPO_Draft = "N" Then
                                    strSQL = "Update ""@GEGRPO"" Set ""U_GRPOEntry""='',""Status""='O' where ""DocEntry""='" & objUDFFormID.Items.Item("U_GEGR").Specific.String & "'"
                                    objRS.DoQuery(strSQL)
                                End If
                                Update_GateEntry_Status(objForm.Items.Item("TVer").Specific.String, "Y", GRPOEntry)
                            Else
                                'If objUDFFormID.Items.Item("U_GEGR").Specific.String <> "" Or objUDFFormID.Items.Item("U_GEEntry").Specific.String <> "" Then Exit Sub

                                objRS = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                strSQL = "Update ""@MIGTIN"" Set ""U_trgtentry""= case when ""U_trgtentry"" is null then " & GRPOEntry & " else ""U_trgtentry""||','||" & GRPOEntry & " End,""U_Prostat""='4'  where ""DocEntry""='" & objUDFFormID.Items.Item("U_gever").Specific.String & "'"
                                objRS.DoQuery(strSQL)
                                If GE_Inward_GRPO_Draft = "N" Then
                                    strSQL = "Update ""@GEGRPO"" Set ""U_GRPOEntry""=case when ""U_GRPOEntry"" is null then " & GRPOEntry & " else ""U_GRPOEntry""||','||" & GRPOEntry & " End,""Status""='C' where ""DocEntry""='" & objUDFFormID.Items.Item("U_GEGR").Specific.String & "'"
                                    objRS.DoQuery(strSQL)
                                End If
                                Update_GateEntry_Status(objForm.Items.Item("TVer").Specific.String, "N", GRPOEntry)
                            End If
                        End If

                End Select

            End If
        Catch ex As Exception

        End Try

    End Sub

    Private Sub Update_GateEntry_Status(ByVal GEEntry As String, ByVal Cancel_Status As String, ByVal GRPOEntry As String)
        Try
            If GEEntry = "" Then Exit Sub
            objRS = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            If Cancel_Status = "Y" Then
                If objAddOn.HANA Then
                    strSQL = "Update T0 Set ""U_Linestat""= 'O' from ""@MIGTIN1"" T0 "
                    strSQL += vbCrLf + "inner join (Select ifnull(Sum(B.""Quantity""),0) ""GRPO Qty"",A.""U_gever"",B.""ItemCode"" from OPDN A join PDN1 B On A.""DocEntry""=B.""DocEntry"""
                    strSQL += vbCrLf + "and A.""CANCELED"" in ('Y','C') and A.""U_gever""='" & GEEntry & "' group by A.""U_gever"",B.""ItemCode"") as T1 on T1.""ItemCode""=T0.""U_itemcode"""
                    strSQL += vbCrLf + "and T1.""U_gever""=T0.""DocEntry"" where T0.""DocEntry""='" & GEEntry & "' and T0.""U_Linestat""='C' "
                    objRS.DoQuery(strSQL)
                    strSQL = "Update T0 Set ""Status""='O' from ""@MIGTIN"" T0 where T0.""DocEntry""='" & GEEntry & "' and T0.""Status""='C'"
                    objRS.DoQuery(strSQL)
                Else
                    strSQL = "Update T0 Set U_Linestat= 'O' from [@MIGTIN1] T0 "
                    strSQL += vbCrLf + "inner join (Select isnull(Sum(B.Quantity),0) GRPO Qty,B.BaseType,B.BaseEntry,B.BaseLine,A.U_gever from OPDN A join PDN1 B On A.DocEntry=B.DocEntry"
                    strSQL += vbCrLf + "and A.CANCELED in ('Y','C') group by B.BaseType,B.BaseEntry,B.BaseLine,A.U_gever) as T1 on T1.BaseType=T0.U_basetype and T1.BaseEntry=T0.U_basentry  "
                    strSQL += vbCrLf + "and T1.BaseLine=T0.U_baseline and T1.U_gever=T0.DocEntry where T0.DocEntry='" & GEEntry & "' and T0.U_Linestat='C' "
                    objRS.DoQuery(strSQL)
                    strSQL = "Update T0 Set Status='O' from [@MIGTIN] T0 where T0.DocEntry='" & GEEntry & "' and T0.Status='C'"
                    objRS.DoQuery(strSQL)
                End If
            Else
                If objAddOn.HANA Then
                    strSQL = " Update T0 Set ""U_Linestat""= Case when (T0.""U_qty""-T1.""GRPO Qty"")<=0 then 'C' Else 'O' End from ""@MIGTIN1"" T0 "
                    strSQL += vbCrLf + "inner join (Select ifnull(Sum(B.""Quantity""),0) ""GRPO Qty"",B.""BaseType"",B.""BaseEntry"",B.""BaseLine"",A.""U_gever"" from OPDN A join PDN1 B on A.""DocEntry""=B.""DocEntry""" 'where ""DocEntry""=" & GRPOEntry & "
                    strSQL += vbCrLf + "where ""CANCELED"" ='N' group by B.""BaseType"",B.""BaseEntry"",B.""BaseLine"",A.""U_gever"") as T1"
                    strSQL += vbCrLf + "on T1.""BaseType""=T0.""U_basetype"" and T1.""BaseEntry""=T0.""U_basentry"" and T1.""U_gever""=T0.""DocEntry"""
                    strSQL += vbCrLf + "and T1.""BaseLine""=T0.""U_baseline"" where T0.""DocEntry""='" & GEEntry & "' and T0.""U_Linestat""='O' "
                    objRS.DoQuery(strSQL)
                    strSQL = "Update T0 Set ""Status""=Case when T1.""U_Linestat""='C' then 'C' Else 'O' End from ""@MIGTIN"" T0 inner join "
                    strSQL += vbCrLf + "(Select Top 1 B.""U_Linestat"",B.""DocEntry"" from ""@MIGTIN1"" B where B.""DocEntry""='" & GEEntry & "' Order by B.""U_Linestat"" desc ) as T1"
                    strSQL += vbCrLf + "on T1.""DocEntry""=T0.""DocEntry""  where T0.""DocEntry""='" & GEEntry & "' and T0.""Status""='O'"
                    objRS.DoQuery(strSQL)
                Else
                    strSQL = " Update T0 Set U_Linestat= Case when (T0.U_qty-T1.GRPO Qty)<=0 then 'C' Else 'O' End from [@MIGTIN1] T0 "
                    strSQL += vbCrLf + "inner join (Select isnull(Sum(B.Quantity),0) GRPO Qty,B.BaseType,B.BaseEntry,B.BaseLine,A.U_gever from OPDN A join PDN1 B on A.DocEntry=B.DocEntry" 'where DocEntry=" & GRPOEntry & "
                    strSQL += vbCrLf + "where CANCELED ='N' group by B.BaseType,B.BaseEntry,B.BaseLine,A.U_gever) as T1"
                    strSQL += vbCrLf + "on T1.BaseType=T0.U_basetype and T1.BaseEntry=T0.U_basentry and T1.U_gever=T0.DocEntry"
                    strSQL += vbCrLf + "and T1.BaseLine=T0.U_baseline where T0.DocEntry='" & GEEntry & "' and T0.U_Linestat='O' "
                    objRS.DoQuery(strSQL)
                    strSQL = "Update T0 Set Status=Case when T1.U_Linestat='C' then 'C' Else 'O' End from [@MIGTIN] T0 inner join "
                    strSQL += vbCrLf + "(Select Top 1 B.U_Linestat,B.DocEntry from [@MIGTIN1] B where B.DocEntry='" & GEEntry & "' Order by B.U_Linestat desc ) as T1"
                    strSQL += vbCrLf + "on T1.DocEntry=T0.DocEntry  where T0.DocEntry='" & GEEntry & "' and T0.Status='O'"
                    objRS.DoQuery(strSQL)
                End If
            End If
            objRS = Nothing
        Catch ex As Exception

        End Try

    End Sub

    Public Sub Create_CustomizedFields(ByVal FormUID As String)
        Try
            objForm = objAddOn.objApplication.Forms.Item(FormUID)
            'Dim objButton As SAPbouiCOM.Button
            Dim objItem As SAPbouiCOM.Item
            'Try
            '    objButton = objForm.Items.Item("Verify").Specific
            'Catch ex As Exception
            '    objItem = objForm.Items.Add("Verify", SAPbouiCOM.BoFormItemTypes.it_BUTTON)
            '    objItem.Left = objForm.Items.Item("2").Left + 100
            '    objItem.Width = objForm.Items.Item("2").Width
            '    objItem.Top = objForm.Items.Item("2").Top
            '    objItem.Height = 20 'objForm.Items.Item("2").Height
            '    objItem.LinkTo = "2"
            '    objButton = objItem.Specific
            '    objButton.Caption = "Verify GE"
            '    objButton.Item.LinkTo = "2"
            'End Try
            Dim objStatic As SAPbouiCOM.StaticText
            Try
                objStatic = objForm.Items.Item("lgeno").Specific
            Catch ex As Exception
                objItem = objForm.Items.Add("lgeno", SAPbouiCOM.BoFormItemTypes.it_STATIC)
                objItem.Left = objForm.Items.Item("2").Left + 100
                objItem.Width = objForm.Items.Item("2").Width
                objItem.Top = objForm.Items.Item("2").Top
                objItem.Height = 20 'objForm.Items.Item("2").Height
                objItem.LinkTo = "2"
                objStatic = objItem.Specific
                objStatic.Caption = "GE No."
                objStatic.Item.LinkTo = "2"
            End Try
            Dim objText As SAPbouiCOM.EditText
            Try
                objText = objForm.Items.Item("TVer").Specific
            Catch ex As Exception
                objItem = objForm.Items.Add("TVer", SAPbouiCOM.BoFormItemTypes.it_EDIT)
                objItem.Left = objForm.Items.Item("2").Left + 200
                objItem.Width = objForm.Items.Item("2").Width + 10
                objItem.Top = objForm.Items.Item("2").Top
                objItem.Height = objForm.Items.Item("4").Height
                objItem.LinkTo = "lgeno"
                objText = objItem.Specific
                objText.DataBind.SetBound(True, "OPDN", "U_gever")
                objText.Item.LinkTo = "lgeno"
                'objText.Value = "Press Tab"
                'Dim oCFLs As SAPbouiCOM.ChooseFromListCollection
                'oCFLs = objForm.ChooseFromLists
                'Dim oCFL As SAPbouiCOM.ChooseFromList
                'Dim oCFLCreationParams As SAPbouiCOM.ChooseFromListCreationParams
                'oCFLCreationParams = objAddOn.objApplication.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_ChooseFromListCreationParams)
                'oCFLCreationParams.MultiSelection = False
                'oCFLCreationParams.ObjectType = "MIGTIN"
                'oCFLCreationParams.UniqueID = "GE_CFL"
                'oCFL = oCFLs.Add(oCFLCreationParams)
                'objText.ChooseFromListUID = "GE_CFL"
                'objText.ChooseFromListAlias = "DocEntry"
            End Try

            Dim objLink As SAPbouiCOM.LinkedButton
            Try
                objLink = objForm.Items.Item("gelink").Specific
            Catch ex As Exception

                objItem = objForm.Items.Add("gelink", SAPbouiCOM.BoFormItemTypes.it_LINKED_BUTTON)
                objItem.Left = objForm.Items.Item("2").Left + 180
                objItem.Width = 20
                objItem.Top = objForm.Items.Item("2").Top
                objItem.Height = objForm.Items.Item("2").Height
                objLink = objItem.Specific
                'objText.DataBind.SetBound(True, "OPDN", "U_gever")
                objLink.LinkedObjectType = "MIGTIN"
                objItem.LinkTo = "TVer"
            End Try
            objForm.Items.Item("4").Click()
        Catch ex As Exception

        End Try
    End Sub

    Private Function Verify(ByVal FormUID As String) As Boolean
        objForm = objAddOn.objApplication.Forms.Item(FormUID)
        objMatrix = objForm.Items.Item("38").Specific
        For i As Integer = 1 To objMatrix.RowCount
            If objMatrix.Columns.Item("1").Cells.Item(i).Specific.String <> "" Then
                Dim BaseEntry As Long = CLng(objMatrix.Columns.Item("45").Cells.Item(i).Specific.String)
                Dim BaseLine As Integer = CInt(objMatrix.Columns.Item("46").Cells.Item(i).Specific.String)
                Dim BaseQty As Double = CDbl(objMatrix.Columns.Item("11").Cells.Item(i).Specific.String)

                If objAddOn.HANA Then
                    strSQL = "SELECT T0.""DocEntry"" FROM ""@MIGTIN1"" T0 WHERE T0.""U_basentry"" = " & BaseEntry & " AND  T0.""U_baseline"" = " & BaseLine & " AND " &
                        " T0.""U_qty"" = " & BaseQty & " AND T0.""U_basetype"" = 22;"

                Else
                    strSQL = "select T0.DocEntry from [@MIGTIN1] T0 where T0.U_basentry = " & BaseEntry & " and T0.U_baseline= " & BaseLine & " and" &
                   "  T0.U_qty = " & BaseQty & " and T0.U_basetype=22"
                End If
                GEDocEntry = objAddOn.objGenFunc.getSingleValue(strSQL)
                If GEDocEntry = "" Then
                    objForm.Items.Item("TVer").Specific.String = GEDocEntry
                    objAddOn.objApplication.MessageBox("Please check the quantity at line : " & CStr(i))

                    ' objForm.DataSources.DBDataSources.Item("OPDN").SetValue("U_gever", 0, "Open")
                    Return False
                End If

            End If
        Next
        'objAddOn.objApplication.MessageBox("All ok")
        objForm.Items.Item("TVer").Specific.String = CStr(GEDocEntry)
        'objForm.DataSources.DBDataSources.Item("OPDN").SetValue("U_gever", 0, "Verified")

        Return True
    End Function

    Public Function VerifyGRN_FromGateEntry(ByVal FormUID As String, ByVal GateEntry As String) As Boolean
        Try
            Dim Flag As Boolean = False
            objForm = objAddOn.objApplication.Forms.Item(FormUID)
            objMatrix = objForm.Items.Item("38").Specific
            objHeader = objForm.DataSources.DBDataSources.Item("OPDN")
            objLine = objForm.DataSources.DBDataSources.Item("PDN1")
            'If objMatrix.Columns.Item("U_GateQty").Editable = False Then objMatrix.Columns.Item("U_GateQty").Editable = True
            For Row As Integer = 0 To objLine.Size - 1
                If objLine.GetValue("ItemCode", Row) <> "" Then
                    If objAddOn.HANA Then
                        'strSQL = "Select ifnull(T1.""U_qty"",0) as ""GEQty"" from ""@MIGTIN"" T0 join ""@MIGTIN1"" T1 on T0.""DocEntry""=T1.""DocEntry"" where T0.""Status""='O' and T0.""DocEntry""='" & GateEntry & "'" &
                        '    " and T1.""U_basetype""='22' and T1.""U_baseline""='" & objLine.GetValue("BaseLine", Row) & "'" & 'and T1.""U_basenum""='" & objLine.GetValue("BaseRef", Row) & "'
                        '    " and T1.""U_basentry""='" & objLine.GetValue("BaseEntry", Row) & "' and T1.""U_itemcode""='" & objLine.GetValue("ItemCode", Row) & "' and T1.""U_Linestat""='O'"
                        strSQL = "Select ifnull(T1.""U_qty"",0) -(Select ifnull(Sum(T0.""Quantity""),0) from OPDN T2 join PDN1 T0 On T0.""DocEntry""=T2.""DocEntry"" where T0.""BaseType""=T1.""U_basetype"""
                        strSQL += vbCrLf + "and T0.""BaseEntry""=T1.""U_basentry"" and T0.""BaseLine""=T1.""U_baseline"" and T2.""U_gever""=T1.""DocEntry"" and T2.""CANCELED""='N' ) as ""GE Open Qty"" "
                        strSQL += vbCrLf + "from ""@MIGTIN1"" T1 where T1.""DocEntry""='" & GateEntry & "' and T1.""U_basetype""='22' and T1.""U_basentry""='" & objLine.GetValue("BaseEntry", Row) & "' and T1.""U_baseline""='" & objLine.GetValue("BaseLine", Row) & "' "
                        strSQL += vbCrLf + "and T1.""U_itemcode""='" & objLine.GetValue("ItemCode", Row) & "' and T1.""U_Linestat""='O'"
                    Else
                        'strSQL = "Select isnull(T1.U_qty,0) as GEQty from [@MIGTIN] T0 join [@MIGTIN1] T1 on T0.DocEntry=T1.DocEntry where T0.Status='O' and T0.DocEntry=" & GateEntry & "" &
                        '    " and T1.U_basetype='22' and T1.U_baseline='" & objLine.GetValue("BaseLine", Row) & "'" & 'and T1.U_basenum='" & objLine.GetValue("BaseRef", Row) & "' 
                        '    " and T1.U_basentry='" & objLine.GetValue("BaseEntry", Row) & "' and T1.U_itemcode='" & objLine.GetValue("ItemCode", Row) & "' and T1.U_Linestat='O'"
                        strSQL = "Select isnull(T1.U_qty,0) -(Select isnull(Sum(T0.Quantity),0) from OPDN T2 join PDN1 T0 On T0.DocEntry=T2.DocEntry T0 where T0.BaseType=T1.U_basetype"
                        strSQL += vbCrLf + "and T0.BaseEntry=T1.U_basentry and T0.BaseLine=T1.U_baseline and T2.U_gever=T1.DocEntry and T2.CANCELED='N') as GE Open Qty "
                        strSQL += vbCrLf + "from [@MIGTIN1] T1 where T1.DocEntry='" & GateEntry & "' and T1.U_basetype='22' and T1.U_basentry='" & objLine.GetValue("BaseEntry", Row) & "' and T1.U_baseline='" & objLine.GetValue("BaseLine", Row) & "' "
                        strSQL += vbCrLf + "and T1.U_itemcode='" & objLine.GetValue("ItemCode", Row) & "' and T1.U_Linestat='O'"
                    End If
                    Dim Qty As String = objAddOn.objGenFunc.getSingleValue(strSQL)
                    'objLine.SetValue("U_GateQty", Row, Qty)
                    'objLine.SetValue("Quantity", Row, Qty)
                    If Qty <> "" Then objMatrix.Columns.Item("U_GateQty").Cells.Item(Row + 1).Specific.String = Qty
                    If Qty <> "" Then objMatrix.Columns.Item("11").Cells.Item(Row + 1).Specific.String = Qty Else objMatrix.Columns.Item("14").Cells.Item(Row + 1).Specific.String = 0 : Flag = True

                End If
            Next

            'For i As Integer = 1 To objMatrix.VisualRowCount
            '    If objMatrix.Columns.Item("1").Cells.Item(i).Specific.String <> "" Then
            '        If objAddOn.HANA Then
            '            strSQL = "Select ifnull(T1.""U_qty"",0) as ""GEQty"" from ""@MIGTIN"" T0 join ""@MIGTIN1"" T1 on T0.""DocEntry""=T1.""DocEntry"" where T0.""Status""='O' and T0.""DocEntry""='" & GateEntry & "'" &
            '                " and T1.""U_basetype""='22' and T1.""U_baseline""='" & objMatrix.Columns.Item("46").Cells.Item(i).Specific.String & "'" & 'and T1.""U_basenum""='" & objMatrix.Columns.Item("44").Cells.Item(i).Specific.String & "'
            '                " and T1.""U_basentry""='" & objMatrix.Columns.Item("45").Cells.Item(i).Specific.String & "' and T1.""U_itemcode""='" & objMatrix.Columns.Item("1").Cells.Item(i).Specific.String & "'"
            '        Else
            '            strSQL = "Select isnull(T1.U_qty,0) as GEQty from [@MIGTIN] T0 join [@MIGTIN1] T1 on T0.DocEntry=T1.DocEntry where T0.Status='O' and T0.DocEntry=" & GateEntry & "" &
            '                " and T1.U_basetype='22' and T1.U_baseline='" & objMatrix.Columns.Item("46").Cells.Item(i).Specific.String & "'" & 'and T1.U_basenum='" & objMatrix.Columns.Item("44").Cells.Item(i).Specific.String & "'
            '                " and T1.U_basentry='" & objMatrix.Columns.Item("45").Cells.Item(i).Specific.String & "' and T1.U_itemcode='" & objMatrix.Columns.Item("1").Cells.Item(i).Specific.String & "'"

            '        End If
            '        Dim Qty As String = objAddOn.objGenFunc.getSingleValue(strSQL)
            '        If Qty <> "" Then objMatrix.Columns.Item("U_GateQty").Cells.Item(i).Specific.String = Qty
            '        If Qty <> "" Then objMatrix.Columns.Item("11").Cells.Item(i).Specific.String = Qty Else objMatrix.Columns.Item("14").Cells.Item(i).Specific.String = 0 : Flag = True
            '    End If
            'Next
            If Flag = True Then
                objAddOn.objApplication.StatusBar.SetText("Removing the Invalid Gate Entry Items. Please wait... ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
                For i As Integer = objMatrix.VisualRowCount To 1 Step -1
                    If CDbl(objMatrix.Columns.Item("U_GateQty").Cells.Item(i).Specific.String) = 0 Then
                        'objAddOn.objApplication.Menus.Item("1293").Activate()
                        objMatrix.DeleteRow(i)
                    End If
                Next
                'objMatrix.AddRow(objMatrix.VisualRowCount) : objMatrix.ClearRowData(objMatrix.VisualRowCount) : objMatrix.DeleteRow(objMatrix.VisualRowCount)
                objAddOn.objApplication.StatusBar.SetText("Removed the Invalid Gate Entry Items... ", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
            End If



            objMatrix.Columns.Item("1").Cells.Item(1).Click()
            'objMatrix.Columns.Item("U_GateQty").Editable = False
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Function GEVerfication(ByVal FormUID As String) As Boolean
        Try
            objForm = objAddOn.objApplication.Forms.Item(FormUID)
            GEDocEntry = Trim(CStr(objForm.DataSources.DBDataSources.Item("OPDN").GetValue("U_gever", 0)))
            If GEDocEntry = "" Then
                objAddOn.objApplication.SetStatusBarMessage("Please verify with Gate Entry")
                Return False
            End If

            Return True
        Catch ex As Exception

        End Try

    End Function

End Class

