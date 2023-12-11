Public Class clsItemDetails
    Public Const Formtype As String = "ItemDetails"
    Dim objForm As SAPbouiCOM.Form
    Dim IOTypeCount As Integer
    Dim objCombo As SAPbouiCOM.ComboBox
    Dim IOFormUID As String = ""
    Dim objGrid As SAPbouiCOM.Grid
    Dim strSQL As String = ""
    Dim objRS As SAPbobsCOM.Recordset
    Dim ColSort As String

    Public Sub LoadScreen(ByVal CallerFormUID As String, ByVal CallerTypeCount As String, ByVal DocType As String, ByVal CardCode As String, ByVal CutDate As String, ByVal GEEntry As String)
        Try
            IOFormUID = CallerFormUID
            IOTypeCount = CallerTypeCount
            objForm = objAddOn.objUIXml.LoadScreenXML("ItemDetails1.xml", Mukesh.SBOLib.UIXML.enuResourceType.Embeded, Formtype)
            objForm.DataSources.DataTables.Add("Items")
            objGrid = objForm.Items.Item("3").Specific
            objForm.Freeze(True)
            If CallerFormUID.Contains(clsGateOutward.Formtype) Then
                LoadItemDetails(objForm.UniqueID, DocType, Trim(CardCode), CutDate)
            ElseIf CallerFormUID.Contains(clsGateInward.Formtype) Then
                LoadInwardItemDetails(objForm.UniqueID, DocType, Trim(CardCode), CutDate)
            ElseIf CallerFormUID.Contains(clsGRN.formtype) Then
                objForm.Items.Item("2A").Visible = False
                objForm.Items.Item("2B").Visible = False
                GetOpenGE_Transactions(objForm.UniqueID, CardCode, "", "Y")
            ElseIf CallerFormUID.Contains(clsGEToGRPO.Formtype) Then
                'objForm.Items.Item("2A").Visible = False
                'objForm.Items.Item("2B").Visible = False
                'If GEEntry <> "" Then Load_GEToGRPO(objForm.UniqueID, GEEntry) Else GetOpenGE_Transactions(objForm.UniqueID, CardCode, GEEntry)
                GetOpenGE_Transactions(objForm.UniqueID, CardCode, GEEntry, "N")
            End If
            objGrid.Columns.Item("DocNum").TitleObject.Click(SAPbouiCOM.BoCellClickType.ct_Double)
            'objGrid.Item.Click()
            'objGrid.Rows.SelectedRows.Add(0)
            objForm.Freeze(False)
            objForm.Visible = True
            'bModal = True
        Catch ex As Exception
            objForm.Freeze(False)
        End Try


    End Sub

    Public Sub ItemEvent(ByVal FormUID As String, ByRef pval As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        Try
            Dim GEntry As String
            Dim oGEForm As SAPbouiCOM.Form
            If pval.BeforeAction Then
                Select Case pval.EventType
                    Case SAPbouiCOM.BoEventTypes.et_CLICK

                        If pval.ItemUID = "101" Then
                            If objGrid.Rows.SelectedRows.Count = 0 Then objAddOn.objApplication.StatusBar.SetText("Please select a row...", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Warning) : BubbleEvent = False : Exit Sub
                            If IOFormUID = clsGRN.formtype Then
                                Dim GRNMatrix As SAPbouiCOM.Matrix
                                GRNMatrix = GForm.Items.Item("38").Specific
                                If GRNMatrix.VisualRowCount = 0 Then BubbleEvent = False
                                GEntry = objGrid.DataTable.GetValue("DocEntry", objGrid.Rows.SelectedRows.Item(0, SAPbouiCOM.BoOrderType.ot_SelectionOrder))
                                If GForm.Title = "Goods Receipt PO - Cancellation" Then Exit Sub
                                If objAddOn.objGRN.VerifyGRN_FromGateEntry(GForm.UniqueID, GEntry) = False Then Exit Sub
                                GForm.Items.Item("TVer").Specific.String = GEntry ' objGrid.DataTable.GetValue("DocEntry", objGrid.Rows.SelectedRows.Item(0, SAPbouiCOM.BoOrderType.ot_SelectionOrder))
                                GRNMatrix.Columns.Item("1").Cells.Item(1).Click()
                                GForm = Nothing
                                objForm.Close()
                            ElseIf IOFormUID = clsGEToGRPO.Formtype Then
                                'GEntry = objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(0, SAPbouiCOM.BoOrderType.ot_SelectionOrder))
                                'GEntry = objGrid.DataTable.GetValue("DocEntry", objGrid.Rows.SelectedRows.Item(0, SAPbouiCOM.BoOrderType.ot_SelectionOrder))
                                GEntry = objGrid.DataTable.GetValue("DocEntry", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(0, SAPbouiCOM.BoOrderType.ot_SelectionOrder)))
                                If GEntry <> "" Then Load_GEToGRPO(FormUID, GEntry)
                            Else
                                LoadDetailsToGT(FormUID)
                            End If

                        End If

                    Case SAPbouiCOM.BoEventTypes.et_MATRIX_LINK_PRESSED
                        If pval.ColUID = "DocNum" Then
                            Try
                                objAddOn.objApplication.Menus.Item("MIGTIN").Activate()
                                oGEForm = objAddOn.objApplication.Forms.ActiveForm
                                oGEForm.Freeze(True)
                                oGEForm.Mode = SAPbouiCOM.BoFormMode.fm_FIND_MODE
                                oGEForm.Items.Item("21").Enabled = True
                                'oGEForm.Items.Item("21").Specific.String = objGrid.DataTable.GetValue("DocNum", objGrid.Rows.SelectedRows.Item(0, SAPbouiCOM.BoOrderType.ot_SelectionOrder))
                                oGEForm.Items.Item("21").Specific.String = objGrid.DataTable.GetValue("DocNum", objGrid.GetDataTableRowIndex(pval.Row))
                                Dim GEDate As Date = objGrid.DataTable.GetValue("Date", objGrid.GetDataTableRowIndex(pval.Row))
                                'Dim GEDate As Date = objGrid.DataTable.GetValue("Date", objGrid.Rows.SelectedRows.Item(0, SAPbouiCOM.BoOrderType.ot_SelectionOrder))
                                strSQL = GEDate.ToString("yyyyMMdd")
                                oGEForm.Items.Item("23").Specific.String = strSQL ' objGrid.DataTable.GetValue("Date", objGrid.Rows.SelectedRows.Item(0, SAPbouiCOM.BoOrderType.ot_SelectionOrder))
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
                        End If

                End Select

            Else
                Select Case pval.EventType
                    Case SAPbouiCOM.BoEventTypes.et_CLICK
                        If pval.ItemUID = "3" And pval.Row <> -1 And pval.ColUID <> "RowsHeader" Then
                            If IOFormUID = clsGRN.formtype Then 'Or IOFormUID = clsGEToGRPO.Formtype
                                objGrid.Rows.SelectedRows.Clear()
                                objGrid.Rows.SelectedRows.Add(pval.Row)
                            ElseIf IOFormUID = clsGEToGRPO.Formtype Then
                                If objGrid.Rows.IsSelected(pval.Row) = True Then
                                    objGrid.Rows.SelectedRows.Remove(pval.Row)
                                Else
                                    objGrid.Rows.SelectedRows.Add(pval.Row)
                                End If
                                'If objGrid.Rows.SelectedRows.Count = 1 Then Exit Sub
                                Dim CurVal As String
                                'CurVal = objGrid.DataTable.GetValue("DocEntry", pval.Row)
                                CurVal = objGrid.DataTable.GetValue("DocEntry", objGrid.GetDataTableRowIndex(pval.Row))
                                For i As Integer = 0 To objGrid.Rows.Count - 1
                                    If objGrid.DataTable.GetValue("DocEntry", objGrid.GetDataTableRowIndex(i)) <> CurVal Then
                                        If objGrid.Rows.IsSelected(i) = True Then objGrid.Rows.SelectedRows.Remove(i)
                                    End If
                                Next
                            Else
                                If IOFormUID = clsGateInward.Formtype Then
                                    oGEForm = objAddOn.objApplication.Forms.GetForm(IOFormUID, IOTypeCount)
                                    If pval.Modifiers = SAPbouiCOM.BoModifiersEnum.mt_None Then objGrid.Rows.SelectedRows.Clear()
                                    If objGrid.Rows.IsSelected(pval.Row) Then
                                        objGrid.Rows.SelectedRows.Remove(pval.Row)
                                    Else
                                        objGrid.Rows.SelectedRows.Add(pval.Row)
                                    End If

                                Else
                                    If objGrid.Rows.IsSelected(pval.Row) = True Then
                                        objGrid.Rows.SelectedRows.Remove(pval.Row)
                                    Else
                                        objGrid.Rows.SelectedRows.Add(pval.Row)
                                    End If
                                End If

                            End If

                        End If
                        If pval.ItemUID = "2A" Then
                            Dim DocEntry(20) As String
                            Dim SelEntry As String
                            If IOFormUID = clsGEToGRPO.Formtype Then
                                For SelRows As Integer = 0 To objGrid.Rows.SelectedRows.Count - 1
                                    'Dim ss As String = objGrid.DataTable.GetValue("DocEntry", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(SelRows, SAPbouiCOM.BoOrderType.ot_SelectionOrder)))
                                    'DocEntry(SelRows) = objGrid.DataTable.GetValue("DocEntry", objGrid.Rows.SelectedRows.Item(SelRows, SAPbouiCOM.BoOrderType.ot_SelectionOrder))
                                    DocEntry(SelRows) = objGrid.DataTable.GetValue("DocEntry", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(SelRows, SAPbouiCOM.BoOrderType.ot_SelectionOrder)))
                                    SelEntry = DocEntry(SelRows)
                                    Exit For
                                Next
                                For SelRows As Integer = 0 To DocEntry.Length - 1
                                    If DocEntry(SelRows) = 0 Then Continue For
                                    If DocEntry(SelRows) = SelEntry Then
                                        For i As Integer = 0 To objGrid.Rows.Count - 1
                                            If objGrid.DataTable.GetValue("DocEntry", objGrid.GetDataTableRowIndex(i)) = DocEntry(SelRows) Then
                                                If objGrid.Rows.IsSelected(i) = False Then objGrid.Rows.SelectedRows.Add(i)
                                            Else
                                                If objGrid.Rows.IsSelected(i) = True Then objGrid.Rows.SelectedRows.Remove(i)
                                            End If
                                        Next
                                    Else
                                        For i As Integer = 0 To objGrid.Rows.Count - 1
                                            If objGrid.DataTable.GetValue("DocEntry", objGrid.GetDataTableRowIndex(i)) = DocEntry(SelRows) Then
                                                If objGrid.Rows.IsSelected(i) = True Then objGrid.Rows.SelectedRows.Remove(i)
                                            End If
                                        Next
                                    End If
                                Next
                            Else
                                For SelRows As Integer = 0 To objGrid.Rows.SelectedRows.Count - 1
                                    'DocEntry(SelRows) = objGrid.DataTable.GetValue("DocEntry", objGrid.Rows.SelectedRows.Item(SelRows, SAPbouiCOM.BoOrderType.ot_SelectionOrder)) 'objGrid.DataTable.GetValue("DocEntry", SelRows)
                                    DocEntry(SelRows) = objGrid.DataTable.GetValue("DocEntry", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(SelRows, SAPbouiCOM.BoOrderType.ot_SelectionOrder)))
                                Next
                                For SelRows As Integer = 0 To DocEntry.Length - 1
                                    If DocEntry(SelRows) = 0 Then Continue For
                                    For i As Integer = 0 To objGrid.Rows.Count - 1
                                        If objGrid.DataTable.GetValue("DocEntry", objGrid.GetDataTableRowIndex(i)) = DocEntry(SelRows) Then
                                            If objGrid.Rows.IsSelected(i) = False Then objGrid.Rows.SelectedRows.Add(i)
                                        End If
                                    Next
                                Next
                            End If

                        ElseIf pval.ItemUID = "2B" Then
                            objGrid.Rows.SelectedRows.Clear()
                            objGrid.Rows.SelectedRows.Add(0)
                        End If
                    Case SAPbouiCOM.BoEventTypes.et_FORM_RESIZE
                        objGrid.AutoResizeColumns()
                    Case SAPbouiCOM.BoEventTypes.et_DOUBLE_CLICK
                        If pval.ItemUID = "3" And pval.ActionSuccess = True Then
                            objGrid.Item.Click()
                            ColSort = pval.ColUID
                            objGrid.Rows.SelectedRows.Add(0)
                        End If
                    Case SAPbouiCOM.BoEventTypes.et_KEY_DOWN
                        If pval.ItemUID = "5" And pval.CharPressed = 9 Then '
                            Dim FindString = objForm.Items.Item("5").Specific.String.ToString.ToUpper
                            oGEForm = objAddOn.objApplication.Forms.GetForm(IOFormUID, IOTypeCount)
                            If FindString <> "" Then
                                If IOFormUID.Contains(clsGateOutward.Formtype) Then
                                    LoadItemDetails(FormUID, Trim(oGEForm.Items.Item("8").Specific.Selected.Value), Trim(oGEForm.Items.Item("10").Specific.string), "", ColSort, FindString)
                                ElseIf IOFormUID.Contains(clsGateInward.Formtype) Then
                                    LoadInwardItemDetails(FormUID, Trim(oGEForm.Items.Item("8").Specific.Selected.Value), Trim(oGEForm.Items.Item("10").Specific.string), "", ColSort, FindString)
                                ElseIf IOFormUID.Contains(clsGEToGRPO.Formtype) Then
                                    GetOpenGE_Transactions(FormUID, Trim(oGEForm.Items.Item("4").Specific.string), Trim(oGEForm.Items.Item("6").Specific.string), "N", ColSort, FindString)
                                End If
                                objGrid.Rows.SelectedRows.Add(0)
                            Else
                                If IOFormUID.Contains(clsGateOutward.Formtype) Then
                                    LoadItemDetails(FormUID, Trim(oGEForm.Items.Item("8").Specific.Selected.Value), Trim(oGEForm.Items.Item("10").Specific.string), "")
                                ElseIf IOFormUID.Contains(clsGateInward.Formtype) Then
                                    LoadInwardItemDetails(FormUID, Trim(oGEForm.Items.Item("8").Specific.Selected.Value), Trim(oGEForm.Items.Item("10").Specific.string), "")
                                ElseIf IOFormUID.Contains(clsGEToGRPO.Formtype) Then
                                    GetOpenGE_Transactions(FormUID, Trim(oGEForm.Items.Item("4").Specific.Selected.Value), Trim(oGEForm.Items.Item("6").Specific.string), "N")
                                End If
                                objGrid.Rows.SelectedRows.Add(0)
                            End If
                            'For i As Integer = 0 To objGrid.Rows.Count - 1
                            '    strSQL = objGrid.DataTable.GetValue(ColSort, objGrid.GetDataTableRowIndex(i)).ToString.ToUpper
                            '    'If objGrid.DataTable.GetValue(ColSort, objGrid.GetDataTableRowIndex(i)) Like FindString Or objGrid.DataTable.GetValue(ColSort, objGrid.GetDataTableRowIndex(i)) Like FindString & "*" Or objGrid.DataTable.GetValue(ColSort, objGrid.GetDataTableRowIndex(i)) Like "*" & FindString & "*" Then
                            '    If strSQL.Contains(FindString) Then
                            '        'objGrid.Rows.SelectedRows.Clear()
                            '        objGrid.Rows.SelectedRows.Add(i)
                            '        Exit For
                            '    End If
                            'Next
                        End If
                End Select

            End If
        Catch ex As Exception
            'objAddOn.objApplication.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try

    End Sub

    Private Sub LoadDetailsToGT(ByVal FormUID As String)
        Try
            Dim OutwardForm As SAPbouiCOM.Form
            Dim OutwardMatrix As SAPbouiCOM.Matrix
            Dim DBLine As String = ""
            If IOFormUID.Contains(clsGateOutward.Formtype) Then
                DBLine = "@MIGTOT1"
            ElseIf IOFormUID.Contains(clsGateInward.Formtype) Then
                DBLine = "@MIGTIN1"
            End If
            OutwardForm = objAddOn.objApplication.Forms.GetForm(IOFormUID, IOTypeCount)
            OutwardMatrix = OutwardForm.Items.Item("36").Specific
            objForm = objAddOn.objApplication.Forms.Item(FormUID)
            objGrid = objForm.Items.Item("3").Specific
            OutwardForm.DataSources.DBDataSources.Item(DBLine).Clear()

            For i As Integer = objGrid.Rows.SelectedRows.Count To 1 Step -1
                'If objGrid.Rows.IsSelected(i) = True Then
                With OutwardForm.DataSources.DBDataSources.Item(DBLine)
                    .InsertRecord(0)
                    .SetValue("LineId", 0, i)
                    .SetValue("U_basetype", 0, objGrid.DataTable.GetValue("ObjType", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(i - 1, SAPbouiCOM.BoOrderType.ot_SelectionOrder))))
                    .SetValue("U_basenum", 0, objGrid.DataTable.GetValue("DocNum", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(i - 1, SAPbouiCOM.BoOrderType.ot_SelectionOrder))))
                    .SetValue("U_basentry", 0, objGrid.DataTable.GetValue("DocEntry", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(i - 1, SAPbouiCOM.BoOrderType.ot_SelectionOrder))))
                    .SetValue("U_baseline", 0, CStr(objGrid.DataTable.GetValue("LineNum", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(i - 1, SAPbouiCOM.BoOrderType.ot_SelectionOrder)))))
                    .SetValue("U_itemcode", 0, objGrid.DataTable.GetValue("ItemCode", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(i - 1, SAPbouiCOM.BoOrderType.ot_SelectionOrder))))
                    .SetValue("U_itemdesc", 0, objGrid.DataTable.GetValue("Dscription", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(i - 1, SAPbouiCOM.BoOrderType.ot_SelectionOrder))))
                    '.SetValue("U_itemdet", 0, objGrid.DataTable.GetValue("Dscription", objGrid.Rows.SelectedRows.Item(i - 1, SAPbouiCOM.BoOrderType.ot_SelectionOrder)))
                    .SetValue("U_itemdet1", 0, objGrid.DataTable.GetValue("Details", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(i - 1, SAPbouiCOM.BoOrderType.ot_SelectionOrder))))
                    .SetValue("U_orderqty", 0, objGrid.DataTable.GetValue("Quantity", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(i - 1, SAPbouiCOM.BoOrderType.ot_SelectionOrder))))
                    .SetValue("U_pendqty", 0, objGrid.DataTable.GetValue("PendingQty", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(i - 1, SAPbouiCOM.BoOrderType.ot_SelectionOrder))))
                    .SetValue("U_gateqty", 0, objGrid.DataTable.GetValue("GateQty", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(i - 1, SAPbouiCOM.BoOrderType.ot_SelectionOrder))))
                    .SetValue("U_qty", 0, objGrid.DataTable.GetValue("PendingQty", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(i - 1, SAPbouiCOM.BoOrderType.ot_SelectionOrder))))
                    .SetValue("U_unitpric", 0, objGrid.DataTable.GetValue("Price", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(i - 1, SAPbouiCOM.BoOrderType.ot_SelectionOrder))))
                    .SetValue("U_linetot", 0, CDbl(objGrid.DataTable.GetValue("PendingQty", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(i - 1, SAPbouiCOM.BoOrderType.ot_SelectionOrder)))) * CDbl(objGrid.DataTable.GetValue("Price", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(i - 1, SAPbouiCOM.BoOrderType.ot_SelectionOrder)))))
                End With
                'End If
            Next
            If DBLine = "@MIGTIN1" Then
                OutwardMatrix.Columns.Item("6").Editable = True
                OutwardMatrix.Columns.Item("9").Editable = True
                OutwardMatrix.Columns.Item("13").Editable = True
            End If
            'For i As Integer = objGrid.Rows.Count To 1 Step -1
            '    If objGrid.DataTable.GetValue("Select", i - 1) = "Y" Then
            '        With OutwardForm.DataSources.DBDataSources.Item(DBLine)
            '            .InsertRecord(0)
            '            .SetValue("LineId", 0, i)
            '            .SetValue("U_basetype", 0, objGrid.DataTable.GetValue("ObjType", i - 1))
            '            .SetValue("U_basenum", 0, objGrid.DataTable.GetValue("DocNum", i - 1))
            '            .SetValue("U_basentry", 0, objGrid.DataTable.GetValue("DocEntry", i - 1))
            '            .SetValue("U_baseline", 0, CStr(objGrid.DataTable.GetValue("LineNum", i - 1)))
            '            .SetValue("U_itemcode", 0, objGrid.DataTable.GetValue("ItemCode", i - 1))
            '            .SetValue("U_itemdesc", 0, objGrid.DataTable.GetValue("Dscription", i - 1))
            '            '  .SetValue("U_itemdet", 0, objGrid.DataTable.GetValue("Dscription", i - 1))
            '            .SetValue("U_itemdet1", 0, objGrid.DataTable.GetValue("Details", i - 1))
            '            .SetValue("U_orderqty", 0, objGrid.DataTable.GetValue("Quantity", i - 1))
            '            .SetValue("U_pendqty", 0, objGrid.DataTable.GetValue("PendQty", i - 1))
            '            .SetValue("U_qty", 0, objGrid.DataTable.GetValue("PendQty", i - 1))
            '            .SetValue("U_unitpric", 0, objGrid.DataTable.GetValue("Price", i - 1))
            '            .SetValue("U_linetot", 0, CDbl(objGrid.DataTable.GetValue("Quantity", i - 1)) * CDbl(objGrid.DataTable.GetValue("Price", i - 1)))
            '        End With
            '    End If
            'Next
            OutwardMatrix.LoadFromDataSourceEx()
            OutwardMatrix.AutoResizeColumns()
            objForm.Close()
        Catch ex As Exception
            objAddOn.objApplication.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
    End Sub

    Private Sub Load_GEToGRPO(ByVal FormUID As String, ByVal GEEntry As String)
        Try
            Dim OutwardForm As SAPbouiCOM.Form
            Dim OutwardMatrix As SAPbouiCOM.Matrix
            Dim POE, POL As String
            OutwardForm = objAddOn.objApplication.Forms.GetForm(IOFormUID, IOTypeCount)
            OutwardMatrix = OutwardForm.Items.Item("8").Specific
            objForm = objAddOn.objApplication.Forms.Item(FormUID)
            objGrid = objForm.Items.Item("3").Specific
            Dim odbdsDetails, odbdsHeader As SAPbouiCOM.DBDataSource
            odbdsHeader = OutwardForm.DataSources.DBDataSources.Item("@GEGRPO")
            odbdsDetails = OutwardForm.DataSources.DBDataSources.Item("@GEGRPO1")
            OutwardMatrix.Clear()
            odbdsDetails.Clear()

            GForm.Items.Item("6").Specific.String = GEEntry
            For SelRows As Integer = 0 To objGrid.Rows.SelectedRows.Count - 1
                'POE = POE + objGrid.DataTable.GetValue("PO Entry", objGrid.Rows.SelectedRows.Item(SelRows, SAPbouiCOM.BoOrderType.ot_SelectionOrder)) + "," 'objGrid.DataTable.GetValue("DocEntry", SelRows)
                'POL = POL + objGrid.DataTable.GetValue("PO Line", objGrid.Rows.SelectedRows.Item(SelRows, SAPbouiCOM.BoOrderType.ot_SelectionOrder)) + "," 'objGrid.DataTable.GetValue("DocEntry", SelRows)

                POE = POE + objGrid.DataTable.GetValue("PO Entry", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(SelRows, SAPbouiCOM.BoOrderType.ot_SelectionOrder))) + ","
                POL = POL + objGrid.DataTable.GetValue("PO Line", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(SelRows, SAPbouiCOM.BoOrderType.ot_SelectionOrder))) + ","
            Next

            POE = POE.Remove(POE.Length - 1)
            POL = POL.Remove(POL.Length - 1)
            'Dim POLine() As String = POL.Split(",")
            If objAddOn.HANA Then
                strSQL = "Select T1.""LineId"",T0.""DocEntry"" ""GE Entry"",T0.""DocNum"" ""GE Num"",To_Varchar(T0.""U_docdate"",'yyyyMMdd') ""GE Date"",T0.""U_partyid"" ""BPCode"",T1.""U_basenum"" ""PO Num"",T1.""U_baseline"" ""PO Line"","
                strSQL += vbCrLf + "T1.""U_basentry"" ""PO Entry"",T1.""U_itemcode"" ""ItemCode"",T1.""U_itemdesc"" ""ItemName"",T1.""U_orderqty"" ""PO Qty"","
                strSQL += vbCrLf + "T1.""U_qty"" ""Gate Qty"",T1.""U_qty"" -ifnull((Select sum(B.""U_Qty"") from ""@GEGRPO"" A join ""@GEGRPO1"" B on A.""DocEntry""=B.""DocEntry"""
                strSQL += vbCrLf + "where A.""Canceled""='N' and B.""U_GEEntry""=T0.""DocEntry"" and B.""U_PoEntry""=T1.""U_basentry"" and B.""U_PoLine""=T1.""U_baseline"" and B.""U_ItemCode""=T1.""U_itemcode"" ),0)""Open Qty"","
                strSQL += vbCrLf + "T1.""U_uom"" ""Uom"",(Select ifnull(Sum(B.""Quantity""),0) from OPDN A join PDN1 B On A.""DocEntry""= B.""DocEntry"" where A.""CANCELED""='N' and A.""U_GEGR""=T0.""U_GEGR""  and T1.""U_basentry""=B.""BaseEntry"" and T1.""U_baseline""=B.""BaseLine"") ""GRPO Qty"","
                strSQL += vbCrLf + "(Select ""DocDate"" from OPOR where ""DocEntry""=T1.""U_basentry"") ""PO Date"",(Select B.""WhsCode"" from POR1 B where T1.""U_basentry""=B.""DocEntry"" and T1.""U_baseline""=B.""LineNum"") ""PO Whse"""
                strSQL += vbCrLf + "from ""@MIGTIN"" T0 join ""@MIGTIN1"" T1 on T0.""DocEntry""=T1.""DocEntry"" where T0.""DocEntry""='" & GEEntry & "' and T1.""U_basentry"" in (" & POE & ")  and T1.""U_baseline"" in (" & POL & ")"
                ''strSQL += vbCrLf + "and T0.""DocEntry"" not in (Select A.""U_GEEntry"" from ""@GEGRPO"" A join ""@GEGRPO1"" B on A.""DocEntry""=B.""DocEntry"" where A.""U_GRPOEntry"" is not null)"
            Else
                strSQL = "Select T1.LineId,T0.DocEntry [GE Entry],T0.DocNum [GE Num],Format(T0.U_docdate,'yyyyMMdd') [GE Date],T0.U_partyid BPCode,T1.U_basenum [PO Num],T1.U_baseline [PO Line],"
                strSQL += vbCrLf + "T1.U_basentry [PO Entry],T1.U_itemcode ItemCode,T1.U_itemdesc ItemName,T1.U_orderqty [PO Qty],"
                strSQL += vbCrLf + "T1.U_qty [Gate Qty],T1.U_qty -isnull((Select sum(B.U_Qty) from [@GEGRPO] A join [@GEGRPO1] B on A.DocEntry=B.DocEntry"
                strSQL += vbCrLf + "where A.Canceled='N' and B.U_GEEntry=T0.DocEntry and B.U_PoEntry=T1.U_basentry and B.U_PoLine=T1.U_baseline and B.U_ItemCode=T1.U_itemcode ),0) [Open Qty],"
                strSQL += vbCrLf + "T1.U_uom Uom,(Select isnull(Sum(B.Quantity),0) from OPDN A join PDN1 B On A.DocEntry= B.DocEntry where A.CANCELED='N' and A.U_GEGR=T0.U_GEGR  and T1.U_basentry=B.BaseEntry and T1.U_baseline=B.BaseLine) [GRPO Qty],"
                strSQL += vbCrLf + "(Select DocDate from OPOR where DocEntry=T1.U_basentry) [PO Date],(Select B.WhsCode from POR1 B where T1.U_basentry=B.DocEntry and T1.U_baseline=B.LineNum) [PO Whse]"
                strSQL += vbCrLf + "from [@MIGTIN] T0 join [@MIGTIN1] T1 on T0.DocEntry=T1.DocEntry where T0.DocEntry='" & GEEntry & "' and T1.U_basentry in (" & POE & ")  and T1.U_baseline in (" & POL & ")"
                ''strSQL += vbCrLf + "and T0.DocEntry not in (Select A.U_GEEntry from [@GEGRPO A join [@GEGRPO1 B on A.DocEntry=B.DocEntry where A.U_GRPOEntry is not null)"
            End If

            objRS = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            objRS.DoQuery(strSQL)
            If objRS.RecordCount > 0 Then
                odbdsHeader.SetValue("U_BPCode", 0, objRS.Fields.Item("BPCode").Value.ToString)
                'odbdsHeader.SetValue("U_GEEntry", 0, objRS.Fields.Item("GE Num").Value.ToString)
                For i As Integer = 0 To objRS.RecordCount - 1
                    Dim ss As String = objRS.Fields.Item("LineId").Value.ToString
                    With odbdsDetails
                        OutwardMatrix.AddRow()
                        OutwardMatrix.GetLineData(OutwardMatrix.VisualRowCount)
                        .SetValue("LineId", 0, i + 1)
                        .SetValue("U_GENum", 0, objRS.Fields.Item("GE Num").Value.ToString)
                        .SetValue("U_GEEntry", 0, objRS.Fields.Item("GE Entry").Value.ToString)
                        .SetValue("U_GEDate", 0, objRS.Fields.Item("GE Date").Value.ToString)
                        .SetValue("U_ItemCode", 0, objRS.Fields.Item("ItemCode").Value.ToString)
                        .SetValue("U_ItemName", 0, objRS.Fields.Item("ItemName").Value.ToString)
                        .SetValue("U_GEQty", 0, objRS.Fields.Item("Gate Qty").Value.ToString)
                        .SetValue("U_GRPOQty", 0, objRS.Fields.Item("GRPO Qty").Value.ToString)
                        .SetValue("U_Qty", 0, objRS.Fields.Item("Open Qty").Value.ToString)
                        .SetValue("U_OpenQty", 0, objRS.Fields.Item("Open Qty").Value.ToString)
                        .SetValue("U_Whse", 0, objRS.Fields.Item("PO Whse").Value.ToString)
                        .SetValue("U_PoEntry", 0, objRS.Fields.Item("PO Entry").Value.ToString)
                        .SetValue("U_PoLine", 0, objRS.Fields.Item("PO Line").Value.ToString)
                        .SetValue("U_PoQty", 0, objRS.Fields.Item("PO Qty").Value.ToString)
                        '.SetValue("U_PoDate", 0, objRS.Fields.Item("PO Date").Value.ToString)
                        .SetValue("U_Uom", 0, objRS.Fields.Item("Uom").Value.ToString)
                        OutwardMatrix.SetLineData(OutwardMatrix.VisualRowCount)
                    End With
                    objRS.MoveNext()
                Next

            End If

            'OutwardMatrix.LoadFromDataSourceEx()
            OutwardMatrix.AutoResizeColumns()
            objForm.Close()
        Catch ex As Exception
            objAddOn.objApplication.StatusBar.SetText(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
    End Sub

    Private Function ReturnQueryOutward(ByVal doctype As String) As String
        Return ""
    End Function

    Public Sub LoadItemDetails(ByVal FormUID As String, ByVal DType As String, ByVal CardCode As String, ByVal CutoffDate As String, Optional ByVal LikeFieldName As String = "", Optional ByVal LikeText As String = "")
        Try
            objForm = objAddOn.objApplication.Forms.Item(FormUID)
            objGrid = objForm.Items.Item("3").Specific
            If objAddOn.HANA Then
                Select Case Trim(DType)
                    Case "SI" 'sales invoice
                        strSQL = "Select * from (SELECT '' AS ""Select"", (SELECT  ""DocNum"" FROM OINV WHERE ""DocEntry"" = T0.""DocEntry"") AS ""DocNum"", T0.""DocEntry"", T0.""LineNum"", "
                        strSQL += vbCrLf + "T0.""ItemCode"", T0.""Dscription"",IFNULL(T0.""Text"",'') AS ""Details"", T0.""Quantity"", T0.""Price"", T0.""LineTotal"", T0.""ObjType"", "
                        strSQL += vbCrLf + "T0.""Quantity"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTOT1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" = cast (T0.""DocEntry"" as varchar) AND ""U_baseline"" = T0.""LineNum"") AS ""PendingQty"", "
                        strSQL += vbCrLf + "(SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTOT1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" = cast (T0.""DocEntry"" as varchar) AND ""U_baseline"" = T0.""LineNum"") AS ""GateQty"""
                        strSQL += vbCrLf + "FROM INV1 T0 WHERE T0.""Quantity"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTOT1"" WHERE ""U_basetype"" = T0.""ObjType""  "
                        strSQL += vbCrLf + "AND ""U_basentry"" = cast (T0.""DocEntry"" as varchar) AND ""U_baseline"" = T0.""LineNum"") > 0 AND T0.""BaseCard"" = '" & CardCode & "' AND T0.""DocDate"">='" & CutoffDate & "') A Where A.""Select""=''"
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '%" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "') "

                    Case "NR", "RT" 'Delivery

                        strSQL = "Select * from (SELECT '' AS ""Select"", (SELECT  ""DocNum"" FROM ODLN WHERE ""DocEntry"" = T0.""DocEntry"") AS ""DocNum"", T0.""DocEntry"", T0.""LineNum"", "
                        strSQL += vbCrLf + "T0.""ItemCode"", T0.""Dscription"",IFNULL(T0.""Text"",'') AS ""Details"", T0.""Quantity"", T0.""Price"", T0.""LineTotal"", T0.""ObjType"", "
                        strSQL += vbCrLf + "T0.""Quantity"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTOT1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" = cast (T0.""DocEntry"" as varchar) AND ""U_baseline"" = T0.""LineNum"") AS ""PendingQty"", "
                        strSQL += vbCrLf + "(SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTOT1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" = cast (T0.""DocEntry"" as varchar) AND ""U_baseline"" = T0.""LineNum"") AS ""GateQty"""
                        strSQL += vbCrLf + "FROM DLN1 T0 join ODLN T1 On T0.""DocEntry""=T1.""DocEntry"" WHERE T0.""Quantity"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTOT1"" WHERE ""U_basetype"" = T0.""ObjType""  "
                        strSQL += vbCrLf + "AND ""U_basentry"" = cast (T0.""DocEntry"" as varchar) AND ""U_baseline"" = T0.""LineNum"") > 0 AND T0.""BaseCard"" = '" & CardCode & "' AND T0.""DocDate"">='" & CutoffDate & "'"
                        If DType = "NR" Then
                            strSQL += vbCrLf + "and T1.""U_TransTyp""='NRDC'"
                        ElseIf DType = "RT" Then
                            strSQL += vbCrLf + "and T1.""U_TransTyp""='RDC'"
                        End If
                        strSQL += vbCrLf + ") A Where A.""Select""='' "
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '%" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "') "
                    Case "MO" 'goods issue

                        strSQL = "Select * from (SELECT '' AS ""Select"", (SELECT  ""DocNum"" FROM OIGE WHERE ""DocEntry"" = T0.""DocEntry"") AS ""DocNum"", T0.""DocEntry"", T0.""LineNum"", "
                        strSQL += vbCrLf + "T0.""ItemCode"", T0.""Dscription"",IFNULL(T0.""Text"",'') AS ""Details"", T0.""Quantity"", T0.""Price"", T0.""LineTotal"", T0.""ObjType"", "
                        strSQL += vbCrLf + "T0.""Quantity"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTOT1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" = cast (T0.""DocEntry"" as varchar) AND ""U_baseline"" = T0.""LineNum"") AS ""PendingQty"", "
                        strSQL += vbCrLf + "(SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTOT1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" = cast (T0.""DocEntry"" as varchar) AND ""U_baseline"" = T0.""LineNum"") AS ""GateQty"""
                        strSQL += vbCrLf + "FROM IGE1 T0 WHERE T0.""Quantity"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTOT1"" WHERE ""U_basetype"" = T0.""ObjType""  "
                        strSQL += vbCrLf + "AND ""U_basentry"" = cast (T0.""DocEntry"" as varchar) AND ""U_baseline"" = T0.""LineNum"") > 0  AND T0.""DocDate"">='" & CutoffDate & "') A Where A.""Select""='' " 'AND T0.""BaseCard"" = '" & CardCode & "';"
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '%" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "') "

                    Case "SR" 'goods return

                        strSQL = "Select * from (SELECT '' AS ""Select"", (SELECT  ""DocNum"" FROM ORPD WHERE ""DocEntry"" = T0.""DocEntry"") AS ""DocNum"", T0.""DocEntry"", T0.""LineNum"", "
                        strSQL += vbCrLf + "T0.""ItemCode"", T0.""Dscription"",IFNULL(T0.""Text"",'') AS ""Details"", T0.""Quantity"", T0.""Price"", T0.""LineTotal"", T0.""ObjType"", "
                        strSQL += vbCrLf + "T0.""Quantity"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTOT1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" = T0.""DocEntry"" AND ""U_baseline"" = T0.""LineNum"") AS ""PendingQty"",0 as ""GateQty"" "
                        strSQL += vbCrLf + "FROM RPD1 T0 WHERE T0.""Quantity"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTOT1"" WHERE ""U_basetype"" = T0.""ObjType""  "
                        strSQL += vbCrLf + "AND ""U_basentry"" = T0.""DocEntry"" AND ""U_baseline"" = T0.""LineNum"") > 0 AND T0.""BaseCard"" = '" & CardCode & "' AND T0.""DocDate"">='" & CutoffDate & "' ) A Where A.""Select""='' "
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '%" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "') "
                    Case "SC" 'AP credit memo

                        strSQL = "Select * from (SELECT '' AS ""Select"", (SELECT  ""DocNum"" FROM ORPC WHERE ""DocEntry"" = T0.""DocEntry"") AS ""DocNum"", T0.""DocEntry"", T0.""LineNum"", "
                        strSQL += vbCrLf + "T0.""ItemCode"", T0.""Dscription"",IFNULL(T0.""Text"",'') AS ""Details"", T0.""Quantity"", T0.""Price"", T0.""LineTotal"", T0.""ObjType"", "
                        strSQL += vbCrLf + "T0.""Quantity"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTOT1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" = cast (T0.""DocEntry"" as varchar) AND ""U_baseline"" = T0.""LineNum"") AS ""PendingQty"", "
                        strSQL += vbCrLf + "(SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTOT1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" = cast (T0.""DocEntry"" as varchar) AND ""U_baseline"" = T0.""LineNum"") AS ""GateQty"" "
                        strSQL += vbCrLf + "FROM RPC1 T0 join ORPC T1 on T0.""DocEntry""=T1.""DocEntry"" WHERE T1.""DocType""='I' and T0.""Quantity"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTOT1"" WHERE ""U_basetype"" = T0.""ObjType""  "
                        strSQL += vbCrLf + "AND ""U_basentry"" = cast (T0.""DocEntry"" as varchar) AND ""U_baseline"" = T0.""LineNum"") > 0 AND T0.""BaseCard"" = '" & CardCode & "' AND T0.""DocDate"">='" & CutoffDate & "' ) A Where A.""Select""='' "
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '%" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "') "

                    Case "JO", "SO", "RW", "RJ", "ST", "IU", "JW" ' stock transfer '"SR"-Commented for SEPL  

                        strSQL = "Select * from (SELECT '' AS ""Select"", (SELECT  ""DocNum"" FROM OWTR WHERE ""DocEntry"" = T0.""DocEntry"") AS ""DocNum"", T0.""DocEntry"", T0.""LineNum"", "
                        strSQL += vbCrLf + "T0.""ItemCode"", T0.""Dscription"",IFNULL(T0.""Text"",'') AS ""Details"", T0.""Quantity"", T0.""Price"", T0.""LineTotal"", T0.""ObjType"", "
                        strSQL += vbCrLf + "T0.""Quantity"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTOT1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" = cast (T0.""DocEntry"" as varchar) AND ""U_baseline"" = T0.""LineNum"") AS ""PendingQty"", "
                        strSQL += vbCrLf + "(SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTOT1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" = cast (T0.""DocEntry"" as varchar) AND ""U_baseline"" = T0.""LineNum"") AS ""GateQty"""
                        strSQL += vbCrLf + "FROM WTR1 T0 WHERE T0.""Quantity"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTOT1"" WHERE ""U_basetype"" = T0.""ObjType""  "
                        strSQL += vbCrLf + "AND ""U_basentry"" = cast (T0.""DocEntry"" as varchar) AND ""U_baseline"" = T0.""LineNum"") > 0 AND T0.""DocDate"">='" & CutoffDate & "' ) A Where A.""Select""='' " 'AND T0.""BaseCard"" = '" & CardCode & "';"
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '%" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "') "

                End Select
            Else
                Select Case Trim(DType)
                    Case "SI" 'sales invoice
                        strSQL = "Select * from (SELECT '' AS [Select], (SELECT  DocNum FROM OINV WHERE DocEntry = T0.DocEntry) AS DocNum, T0.DocEntry, T0.LineNum, "
                        strSQL += vbCrLf + "T0.ItemCode, T0.Dscription,ISNULL(T0.Text,'') AS Details, T0.Quantity, T0.Price, T0.LineTotal, T0.ObjType, "
                        strSQL += vbCrLf + "T0.Quantity - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTOT1] WHERE U_basetype = T0.ObjType AND U_basentry = cast (T0.DocEntry as varchar) AND U_baseline = T0.LineNum) AS PendingQty, "
                        strSQL += vbCrLf + "(SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTOT1] WHERE U_basetype = T0.ObjType AND U_basentry = cast (T0.DocEntry as varchar) AND U_baseline = T0.LineNum) AS GateQty"
                        strSQL += vbCrLf + "FROM INV1 T0 WHERE T0.Quantity - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTOT1] WHERE U_basetype = T0.ObjType  "
                        strSQL += vbCrLf + "AND U_basentry = cast (T0.DocEntry as varchar) AND U_baseline = T0.LineNum) > 0 AND T0.BaseCard = '" & CardCode & "' AND T0.DocDate>='" & CutoffDate & "') A Where A.Select='' "
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(A." & LikeFieldName & ") like '" & LikeText & "%' or UPPER(A." & LikeFieldName & ") like '%" & LikeText & "%' or UPPER(A." & LikeFieldName & ") like '" & LikeText & "') "

                    Case "NR", "RT" 'Delivery

                        strSQL = "Select * from (SELECT '' AS [Select], (SELECT  DocNum FROM ODLN WHERE DocEntry = T0.DocEntry) AS DocNum, T0.DocEntry, T0.LineNum, "
                        strSQL += vbCrLf + "T0.ItemCode, T0.Dscription,ISNULL(T0.Text,'') AS Details, T0.Quantity, T0.Price, T0.LineTotal, T0.ObjType, "
                        strSQL += vbCrLf + "T0.Quantity - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTOT1] WHERE U_basetype = T0.ObjType AND U_basentry = cast (T0.DocEntry as varchar) AND U_baseline = T0.LineNum) AS PendingQty, "
                        strSQL += vbCrLf + "(SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTOT1] WHERE U_basetype = T0.ObjType AND U_basentry = cast (T0.DocEntry as varchar) AND U_baseline = T0.LineNum) AS GateQty"
                        strSQL += vbCrLf + "FROM DLN1 T0 join ODLN T1 On T0.DocEntry=T1.DocEntry WHERE T0.Quantity - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTOT1] WHERE U_basetype = T0.ObjType  "
                        strSQL += vbCrLf + "AND U_basentry = cast (T0.DocEntry as varchar) AND U_baseline = T0.LineNum) > 0 AND T0.BaseCard = '" & CardCode & "' AND T0.DocDate>='" & CutoffDate & "'"
                        If DType = "NR" Then
                            strSQL += vbCrLf + "and T1.U_TransTyp='NRDC'"
                        ElseIf DType = "RT" Then
                            strSQL += vbCrLf + "and T1.U_TransTyp='RDC'"
                        End If
                        strSQL += vbCrLf + ") A Where A.Select='' "
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(A." & LikeFieldName & ") like '" & LikeText & "%' or UPPER(A." & LikeFieldName & ") like '%" & LikeText & "%' or UPPER(A." & LikeFieldName & ") like '" & LikeText & "') "
                    Case "MO" 'goods issue

                        strSQL = "Select * from (SELECT '' AS [Select], (SELECT  DocNum FROM OIGE WHERE DocEntry = T0.DocEntry) AS DocNum, T0.DocEntry, T0.LineNum, "
                        strSQL += vbCrLf + "T0.ItemCode, T0.Dscription,ISNULL(T0.Text,'') AS Details, T0.Quantity, T0.Price, T0.LineTotal, T0.ObjType, "
                        strSQL += vbCrLf + "T0.Quantity - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTOT1] WHERE U_basetype = T0.ObjType AND U_basentry = cast (T0.DocEntry as varchar) AND U_baseline = T0.LineNum) AS PendingQty, "
                        strSQL += vbCrLf + "(SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTOT1] WHERE U_basetype = T0.ObjType AND U_basentry = cast (T0.DocEntry as varchar) AND U_baseline = T0.LineNum) AS GateQty"
                        strSQL += vbCrLf + "FROM IGE1 T0 WHERE T0.Quantity - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTOT1] WHERE U_basetype = T0.ObjType  "
                        strSQL += vbCrLf + "AND U_basentry = cast (T0.DocEntry as varchar) AND U_baseline = T0.LineNum) > 0  AND T0.DocDate>='" & CutoffDate & "') A Where A.Select=''" 'AND T0.BaseCard = '" & CardCode & "';"
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(A." & LikeFieldName & ") like '" & LikeText & "%' or UPPER(A." & LikeFieldName & ") like '%" & LikeText & "%' or UPPER(A." & LikeFieldName & ") like '" & LikeText & "') "
                    Case "SR" 'goods return

                        strSQL = "Select * from (SELECT '' AS [Select], (SELECT  DocNum FROM ORPD WHERE DocEntry = T0.DocEntry) AS DocNum, T0.DocEntry, T0.LineNum, "
                        strSQL += vbCrLf + "T0.ItemCode, T0.Dscription,ISNULL(T0.Text,'') AS Details, T0.Quantity, T0.Price, T0.LineTotal, T0.ObjType, "
                        strSQL += vbCrLf + "T0.Quantity - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTOT1] WHERE U_basetype = T0.ObjType AND U_basentry = T0.DocEntry AND U_baseline = T0.LineNum) AS PendingQty,0 as GateQty "
                        strSQL += vbCrLf + "FROM RPD1 T0 WHERE T0.Quantity - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTOT1] WHERE U_basetype = T0.ObjType  "
                        strSQL += vbCrLf + "AND U_basentry = T0.DocEntry AND U_baseline = T0.LineNum) > 0 AND T0.BaseCard = '" & CardCode & "' AND T0.DocDate>='" & CutoffDate & "' ) A Where A.Select=''"
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(A." & LikeFieldName & ") like '" & LikeText & "%' or UPPER(A." & LikeFieldName & ") like '%" & LikeText & "%' or UPPER(A." & LikeFieldName & ") like '" & LikeText & "') "
                    Case "SC" 'AP credit memo

                        strSQL = "Select * from (SELECT '' AS [Select], (SELECT  DocNum FROM ORPC WHERE DocEntry = T0.DocEntry) AS DocNum, T0.DocEntry, T0.LineNum, "
                        strSQL += vbCrLf + "T0.ItemCode, T0.Dscription,ISNULL(T0.Text,'') AS Details, T0.Quantity, T0.Price, T0.LineTotal, T0.ObjType, "
                        strSQL += vbCrLf + "T0.Quantity - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTOT1] WHERE U_basetype = T0.ObjType AND U_basentry = cast (T0.DocEntry as varchar) AND U_baseline = T0.LineNum) AS PendingQty, "
                        strSQL += vbCrLf + "(SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTOT1] WHERE U_basetype = T0.ObjType AND U_basentry = cast (T0.DocEntry as varchar) AND U_baseline = T0.LineNum) AS GateQty "
                        strSQL += vbCrLf + "FROM RPC1 T0 join ORPC T1 on T0.DocEntry=T1.DocEntry WHERE T1.DocType='I' and T0.Quantity - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTOT1] WHERE U_basetype = T0.ObjType  "
                        strSQL += vbCrLf + "AND U_basentry = cast (T0.DocEntry as varchar) AND U_baseline = T0.LineNum) > 0 AND T0.BaseCard = '" & CardCode & "' AND T0.DocDate>='" & CutoffDate & "') A Where A.Select=''"
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(A." & LikeFieldName & ") like '" & LikeText & "%' or UPPER(A." & LikeFieldName & ") like '%" & LikeText & "%' or UPPER(A." & LikeFieldName & ") like '" & LikeText & "') "
                    Case "JO", "SO", "RW", "RJ", "ST", "IU", "JW" ' stock transfer '"SR"-Commented for SEPL  

                        strSQL = "Select * from (SELECT '' AS [Select], (SELECT  DocNum FROM OWTR WHERE DocEntry = T0.DocEntry) AS DocNum, T0.DocEntry, T0.LineNum, "
                        strSQL += vbCrLf + "T0.ItemCode, T0.Dscription,ISNULL(T0.Text,'') AS Details, T0.Quantity, T0.Price, T0.LineTotal, T0.ObjType, "
                        strSQL += vbCrLf + "T0.Quantity - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTOT1] WHERE U_basetype = T0.ObjType AND U_basentry = cast (T0.DocEntry as varchar) AND U_baseline = T0.LineNum) AS PendingQty, "
                        strSQL += vbCrLf + "(SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTOT1] WHERE U_basetype = T0.ObjType AND U_basentry = cast (T0.DocEntry as varchar) AND U_baseline = T0.LineNum) AS GateQty"
                        strSQL += vbCrLf + "FROM WTR1 T0 WHERE T0.Quantity - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTOT1] WHERE U_basetype = T0.ObjType  "
                        strSQL += vbCrLf + "AND U_basentry = cast (T0.DocEntry as varchar) AND U_baseline = T0.LineNum) > 0 AND T0.DocDate>='" & CutoffDate & "') A Where A.Select=''" 'AND T0.BaseCard = '" & CardCode & "';"
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(A." & LikeFieldName & ") like '" & LikeText & "%' or UPPER(A." & LikeFieldName & ") like '%" & LikeText & "%' or UPPER(A." & LikeFieldName & ") like '" & LikeText & "') "
                End Select
            End If
            objForm.Freeze(True)
            objForm.DataSources.DataTables.Item("Items").ExecuteQuery(strSQL)
            objGrid.DataTable = objForm.DataSources.DataTables.Item("Items")
            objGrid.Columns.Item(0).Type = SAPbouiCOM.BoGridColumnType.gct_CheckBox
            objGrid.RowHeaders.TitleObject.Caption = "#"
            For i As Integer = 0 To objGrid.Columns.Count - 1
                If i = 0 Then objGrid.Columns.Item(i).Visible = False : Continue For
                If objGrid.Columns.Item(i).Editable = True Then objGrid.Columns.Item(i).Editable = False
                If objGrid.Columns.Item(i).TitleObject.Sortable = False Then objGrid.Columns.Item(i).TitleObject.Sortable = True
            Next
            Dim col As SAPbouiCOM.EditTextColumn
            col = objGrid.Columns.Item(2)
            objRS = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            objRS.DoQuery(strSQL)
            col.LinkedObjectType = objRS.Fields.Item("ObjType").Value
            objGrid.AutoResizeColumns()
            objForm.Freeze(False)
        Catch ex As Exception
            objForm.Freeze(False)
        End Try
    End Sub

    Public Sub LoadInwardItemDetails(ByVal FormUID As String, ByVal DType As String, ByVal CardCode As String, ByVal CutoffDate As String, Optional ByVal LikeFieldName As String = "", Optional ByVal LikeText As String = "")
        Try
            objForm = objAddOn.objApplication.Forms.Item(FormUID)
            objGrid = objForm.Items.Item("3").Specific

            If objAddOn.HANA Then
                Select Case Trim(DType)
                    Case "PO", "GR"
                        '        strSQL = "SELECT '' AS ""Select"", (SELECT ""DocNum"" FROM OPOR WHERE ""DocEntry"" = T0.""DocEntry"") AS ""DocNum"", T0.""DocEntry"", T0.""LineNum"", " &
                        '" T0.""ItemCode"", T0.""Dscription"",IFNULL(T0.""Text"",'') AS ""Details"", T0.""Quantity"", T0.""Price"", T0.""LineTotal"", T0.""ObjType"", " &
                        '" T0.""Quantity"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTIN1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" = Cast(T0.""DocEntry"" as Varchar) AND ""U_baseline"" = T0.""LineNum"") AS ""PendQty"" " &
                        '" FROM POR1 T0 WHERE T0.""Quantity"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTIN1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry""  =  Cast(T0.""DocEntry"" as Varchar) " &
                        '" AND ""U_baseline"" = T0.""LineNum"") > 0 AND T0.""LineStatus"" ='O' AND T0.""BaseCard"" = '" & CardCode & "';"

                        strSQL = "Select * from (Select A.*,Case when (A.""Quantity""-A.""GRN Qty"")<=(A.""Quantity""-A.""GateQty"") then (A.""OpenQty""-CASE WHEN (A.""GateQty""-A.""GRN Qty"")>0 THEN (A.""GateQty""-A.""GRN Qty"")ELSE 0 END) Else (A.""Quantity""-A.""GateQty"") End as ""PendingQty"" "
                        strSQL += vbCrLf + "from ( SELECT '' AS ""Select"", (SELECT ""DocNum"" FROM OPOR WHERE ""DocEntry"" = T0.""DocEntry"") AS ""DocNum"", T0.""DocEntry"", (SELECT ""DocDate"" FROM OPOR WHERE ""DocEntry"" = T0.""DocEntry"") AS ""Document Date"", T0.""LineNum"", "  '(A.""Quantity""-A.""GateQty"") as ""PendingQty""
                        strSQL += vbCrLf + "T0.""ItemCode"", T0.""Dscription"",IFNULL(T0.""Text"",'') AS ""Details"", T0.""Quantity"", T0.""OpenQty"",(T0.""Quantity""-T0.""OpenQty"")as ""GRN Qty"",T0.""Price"", T0.""LineTotal"", T0.""ObjType"", "  '" T0.""OpenQty"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTIN1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" = Cast(T0.""DocEntry"" as Varchar) AND ""U_baseline"" = T0.""LineNum"") AS ""PendQty"", " &            
                        'strSQL += vbCrLf + "(SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTIN1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" = Cast(T0.""DocEntry"" as Varchar) AND ""U_baseline"" = T0.""LineNum"") AS ""GateQty"" "
                        strSQL += vbCrLf + "(SELECT IFNULL(SUM(IFNULL(X.""U_qty"", 0)), 0) FROM ""@MIGTIN1"" X left Join ""@MIGTIN"" Y On X.""DocEntry""=Y.""DocEntry"" WHERE Y.""Canceled""='N' and X.""U_basetype"" = T0.""ObjType"" AND X.""U_basentry"" = Cast(T0.""DocEntry"" as Varchar) AND X.""U_baseline"" = T0.""LineNum"") AS ""GateQty"" "
                        strSQL += vbCrLf + "FROM POR1 T0 WHERE T0.""LineStatus"" ='O' AND T0.""BaseCard"" = '" & CardCode & "') A ) B where B.""PendingQty"">0" 'T0.""Quantity"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTIN1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry""  =  Cast(T0.""DocEntry"" as Varchar) " &            " AND ""U_baseline"" = T0.""LineNum"") > 0 AND
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(B.""" & LikeFieldName & """) like '" & LikeText & "%' or UPPER(B.""" & LikeFieldName & """) like '%" & LikeText & "%' or UPPER(B.""" & LikeFieldName & """) like '" & LikeText & "') "
                    Case "SR", "IN"
                        strSQL = "Select * from (SELECT '' AS ""Select"", (SELECT ""DocNum"" FROM OINV WHERE ""DocEntry"" = T0.""DocEntry"") AS ""DocNum"", T0.""DocEntry"", (SELECT ""DocDate"" FROM OINV WHERE ""DocEntry"" = T0.""DocEntry"") AS ""Document Date"",T0.""LineNum"", "
                        strSQL += vbCrLf + "T0.""ItemCode"", T0.""Dscription"",IFNULL(T0.""Text"",'') AS ""Details"", T0.""Quantity"", T0.""Price"", T0.""LineTotal"", T0.""ObjType"", "
                        strSQL += vbCrLf + "T0.""Quantity"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTIN1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" =  Cast(T0.""DocEntry"" as Varchar) AND ""U_baseline"" = T0.""LineNum"") AS ""PendingQty"", "
                        strSQL += vbCrLf + "(SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTIN1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" =  Cast(T0.""DocEntry"" as Varchar) AND ""U_baseline"" = T0.""LineNum"") AS ""GateQty"""
                        strSQL += vbCrLf + "FROM INV1 T0 WHERE T0.""Quantity"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTIN1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" = T0.""DocEntry"" "
                        strSQL += vbCrLf + "AND ""U_baseline"" = T0.""LineNum"") > 0 AND T0.""BaseCard"" = '" & CardCode & "' AND T0.""DocDate"">='" & CutoffDate & "') A Where A.""Select""='' "
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '%" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "') "

                    Case "DR", "WI"
                        strSQL = "Select * from (SELECT '' AS ""Select"", (SELECT ""DocNum"" FROM ODLN WHERE ""DocEntry"" = T0.""DocEntry"") AS ""DocNum"", T0.""DocEntry"",(SELECT ""DocDate"" FROM ODLN WHERE ""DocEntry"" = T0.""DocEntry"") AS ""Document Date"",T0.""LineNum"", "
                        strSQL += vbCrLf + "T0.""ItemCode"", T0.""Dscription"",IFNULL(T0.""Text"",'') AS ""Details"", T0.""Quantity"", T0.""Price"", T0.""LineTotal"", T0.""ObjType"", "
                        strSQL += vbCrLf + "T0.""Quantity"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTIN1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" = Cast(T0.""DocEntry"" as Varchar) AND ""U_baseline"" = T0.""LineNum"") AS ""PendingQty"", "
                        strSQL += vbCrLf + "(SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTIN1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" = Cast(T0.""DocEntry"" as Varchar) AND ""U_baseline"" = T0.""LineNum"") AS ""GateQty"""
                        strSQL += vbCrLf + "FROM DLN1 T0 left join ODLN T1 On T0.""DocEntry""=T1.""DocEntry"" WHERE T0.""Quantity"" - (SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTIN1"" WHERE ""U_basetype"" = T0.""ObjType"" AND ""U_basentry"" = T0.""DocEntry"" "
                        strSQL += vbCrLf + "AND ""U_baseline"" = T0.""LineNum"") > 0 AND T0.""BaseCard"" = '" & CardCode & "' and T1.""U_TransTyp""='RDC' and T0.""LineStatus""='O' AND T0.""DocDate"">='" & CutoffDate & "'"
                        strSQL += vbCrLf + "and T0.""DocEntry"" in (Select ""U_basentry"" from ""@MIGTOT1"" Where ""U_basentry""=T0.""DocEntry"" and ""U_basetype""='15' and ifnull(""U_qty"",0)=T0.""Quantity"") ) A Where A.""Select""=''"
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '%" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "') "

                    Case "DC", "JR", "SR", "WM", "RW", "ST", "SO", "CP", "JO", "JW", "MI", "HR" ' Cash Purchase
                        strSQL = "Select * from (SELECT '' as ""Select"", '0' as ""DocNum"",'0' as ""DocEntry"" ,0 as ""LineNum"",""ItemCode"", ""ItemName""  as  ""Dscription"",""UserText""  as ""Details"",""OnHand"" as ""Quantity"", ""LastPurPrc"" as ""Price"", 0 as ""LineTotal"","
                        strSQL += vbCrLf + "'4' as ""ObjType"", 1 as ""PendingQty"",(SELECT IFNULL(SUM(IFNULL(""U_qty"", 0)), 0) FROM ""@MIGTIN1"" Where ""U_itemcode""=""ItemCode"") as ""GateQty"" FROM OITM Order by ""ItemName"") A Where A.""Select""=''"
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '%" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "') "
                End Select
            Else
                Select Case Trim(DType)
                    Case "PO", "GR"
                        '        strSQL = "SELECT '' AS [Select], (SELECT DocNum FROM OPOR WHERE DocEntry = T0.DocEntry) AS DocNum, T0.DocEntry, T0.LineNum, " &
                        '" T0.ItemCode, T0.Dscription,ISNULL(T0.Text,'') AS Details, T0.Quantity, T0.Price, T0.LineTotal, T0.ObjType, " &
                        '" T0.Quantity - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTIN1 WHERE U_basetype = T0.ObjType AND U_basentry = Cast(T0.DocEntry as Varchar) AND U_baseline = T0.LineNum) AS PendQty " &
                        '" FROM POR1 T0 WHERE T0.Quantity - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTIN1 WHERE U_basetype = T0.ObjType AND U_basentry  =  Cast(T0.DocEntry as Varchar) " &
                        '" AND U_baseline = T0.LineNum) > 0 AND T0.LineStatus ='O' AND T0.BaseCard = '" & CardCode & "';"

                        strSQL = "Select * from (Select A.*,Case when (A.Quantity-A.[GRN Qty])<=(A.Quantity-A.GateQty) then (A.OpenQty-CASE WHEN (A.GateQty-A.[GRN Qty])>0 THEN (A.GateQty-A.[GRN Qty])ELSE 0 END) Else (A.Quantity-A.GateQty) End as PendingQty "
                        strSQL += vbCrLf + "from ( SELECT '' AS [Select], (SELECT DocNum FROM OPOR WHERE DocEntry = T0.DocEntry) AS DocNum, T0.DocEntry, (SELECT DocDate FROM OPOR WHERE DocEntry = T0.DocEntry) AS [Document Date], T0.LineNum, "  '(A.Quantity-A.GateQty) as PendingQty
                        strSQL += vbCrLf + "T0.ItemCode, T0.Dscription,ISNULL(T0.Text,'') AS Details, T0.Quantity, T0.OpenQty,(T0.Quantity-T0.OpenQty)as [GRN Qty],T0.Price, T0.LineTotal, T0.ObjType, "  '" T0.OpenQty - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTIN1 WHERE U_basetype = T0.ObjType AND U_basentry = Cast(T0.DocEntry as Varchar) AND U_baseline = T0.LineNum) AS PendQty, " &            
                        strSQL += vbCrLf + "(SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTIN1] WHERE U_basetype = T0.ObjType AND U_basentry = Cast(T0.DocEntry as Varchar) AND U_baseline = T0.LineNum) AS GateQty "
                        strSQL += vbCrLf + "FROM POR1 T0 WHERE T0.LineStatus ='O' AND T0.BaseCard = '" & CardCode & "') A ) B where B.PendingQty>0" 'T0.Quantity - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTIN1 WHERE U_basetype = T0.ObjType AND U_basentry  =  Cast(T0.DocEntry as Varchar) " &            " AND U_baseline = T0.LineNum) > 0 AND
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(B." & LikeFieldName & ") like '" & LikeText & "%' or UPPER(B." & LikeFieldName & ") like '%" & LikeText & "%' or UPPER(B." & LikeFieldName & ") like '" & LikeText & "') "

                    Case "SR", "IN"
                        strSQL = "Select * from (SELECT '' AS [Select], (SELECT DocNum FROM OINV WHERE DocEntry = T0.DocEntry) AS DocNum, T0.DocEntry, (SELECT DocDate FROM OINV WHERE DocEntry = T0.DocEntry) AS [Document Date],T0.LineNum, "
                        strSQL += vbCrLf + "T0.ItemCode, T0.Dscription,ISNULL(T0.Text,'') AS Details, T0.Quantity, T0.Price, T0.LineTotal, T0.ObjType, "
                        strSQL += vbCrLf + "T0.Quantity - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTIN1] WHERE U_basetype = T0.ObjType AND U_basentry =  Cast(T0.DocEntry as Varchar) AND U_baseline = T0.LineNum) AS PendingQty, "
                        strSQL += vbCrLf + "(SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTIN1] WHERE U_basetype = T0.ObjType AND U_basentry =  Cast(T0.DocEntry as Varchar) AND U_baseline = T0.LineNum) AS GateQty"
                        strSQL += vbCrLf + "FROM INV1 T0 WHERE T0.Quantity - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTIN1] WHERE U_basetype = T0.ObjType AND U_basentry = T0.DocEntry "
                        strSQL += vbCrLf + "AND U_baseline = T0.LineNum) > 0 AND T0.BaseCard = '" & CardCode & "' AND T0.DocDate>='" & CutoffDate & "') A Where A.Select=''"
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(A." & LikeFieldName & ") like '" & LikeText & "%' or UPPER(A." & LikeFieldName & ") like '%" & LikeText & "%' or UPPER(A." & LikeFieldName & ") like '" & LikeText & "') "

                    Case "DR", "WI"
                        strSQL = "Select * from (SELECT '' AS [Select], (SELECT DocNum FROM ODLN WHERE DocEntry = T0.DocEntry) AS DocNum, T0.DocEntry,(SELECT DocDate FROM ODLN WHERE DocEntry = T0.DocEntry) AS [Document Date],T0.LineNum, "
                        strSQL += vbCrLf + "T0.ItemCode, T0.Dscription,ISNULL(T0.Text,'') AS Details, T0.Quantity, T0.Price, T0.LineTotal, T0.ObjType, "
                        strSQL += vbCrLf + "T0.Quantity - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTIN1] WHERE U_basetype = T0.ObjType AND U_basentry = Cast(T0.DocEntry as Varchar) AND U_baseline = T0.LineNum) AS PendingQty, "
                        strSQL += vbCrLf + "(SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTIN1] WHERE U_basetype = T0.ObjType AND U_basentry = Cast(T0.DocEntry as Varchar) AND U_baseline = T0.LineNum) AS GateQty"
                        strSQL += vbCrLf + "FROM DLN1 T0 left join ODLN T1 On T0.DocEntry=T1.DocEntry WHERE T0.Quantity - (SELECT ISNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTIN1] WHERE U_basetype = T0.ObjType AND U_basentry = T0.DocEntry "
                        strSQL += vbCrLf + "AND U_baseline = T0.LineNum) > 0 AND T0.BaseCard = '" & CardCode & "' and T1.U_TransTyp='RDC' and T0.LineStatus='O' AND T0.DocDate>='" & CutoffDate & "'"
                        strSQL += vbCrLf + "and T0.DocEntry in (Select U_basentry from @MIGTOT1 Where U_basentry=T0.DocEntry and U_basetype='15' and isnull(U_qty,0)=T0.Quantity) ) A Where A.Select=''"
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(A." & LikeFieldName & ") like '" & LikeText & "%' or UPPER(A." & LikeFieldName & ") like '%" & LikeText & "%' or UPPER(A." & LikeFieldName & ") like '" & LikeText & "') "

                    Case "DC", "JR", "SR", "WM", "RW", "ST", "SO", "CP", "JO", "JW", "MI", "HR" ' Cash Purchase
                        strSQL = "Select * from (SELECT '' as [Select], '0' as DocNum,'0' as DocEntry ,0 as LineNum,ItemCode, ItemName  as  Dscription,UserText  as Details,OnHand as Quantity, LastPurPrc as Price, 0 as LineTotal,"
                        strSQL += vbCrLf + "'4' as ObjType, 1 as PendingQty,(SELECT IFNULL(SUM(ISNULL(U_qty, 0)), 0) FROM [@MIGTIN1] Where U_itemcode=ItemCode) as GateQty FROM OITM Order by ItemName) A Where A.Select=''"
                        If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(A." & LikeFieldName & ") like '" & LikeText & "%' or UPPER(A." & LikeFieldName & ") like '%" & LikeText & "%' or UPPER(A." & LikeFieldName & ") like '" & LikeText & "') "
                End Select
            End If
            objForm.Freeze(True)
            objForm.DataSources.DataTables.Item("Items").ExecuteQuery(strSQL)
            objGrid.DataTable = objForm.DataSources.DataTables.Item("Items")
            objGrid.Columns.Item(0).Type = SAPbouiCOM.BoGridColumnType.gct_CheckBox
            objGrid.RowHeaders.TitleObject.Caption = "#"
            For i As Integer = 0 To objGrid.Columns.Count - 1
                If i = 0 Then objGrid.Columns.Item(i).Visible = False : Continue For
                objGrid.Columns.Item(i).Editable = False
                objGrid.Columns.Item(i).TitleObject.Sortable = True
            Next
            Dim col As SAPbouiCOM.EditTextColumn
            col = objGrid.Columns.Item(2)
            objRS = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            objRS.DoQuery(strSQL)
            col.LinkedObjectType = objRS.Fields.Item("ObjType").Value
            objForm.Freeze(False)
        Catch ex As Exception
            objForm.Freeze(False)
        End Try

    End Sub

    Private Sub GetOpenGE_Transactions(ByVal FormUID As String, ByVal CardCode As String, ByVal GEEntry As String, ByVal GRPO_Yes_No As String, Optional ByVal LikeFieldName As String = "", Optional ByVal LikeText As String = "")
        Try
            objForm = objAddOn.objApplication.Forms.Item(FormUID)
            objForm.Title = "Open Gate Inward Entries"
            objGrid = objForm.Items.Item("3").Specific
            If GRPO_Yes_No = "Y" Then 'From GRPO 
                If objAddOn.HANA Then
                    strSQL = "Select T0.""DocEntry"",T0.""DocNum"",(Select ""Location"" from OLCT where ""Code""=T0.""U_loc"") ""Location"",T0.""U_type"" ""Type"",T0.""U_partyid"" ""Party Id"",T0.""U_partynm"" ""Party Name"","
                    strSQL += vbCrLf + " T0.""U_docdate"" ""Date"",(Select ""firstName"" ||' '|| ""lastName"" from OHEM where ""empID""=T0.""U_sname"") ""Security Name"","
                    strSQL += vbCrLf + " T0.""U_vehno"" ""Vehicle No."",T0.""U_transnm"" ""Transporter Name"""
                    strSQL += vbCrLf + " from ""@MIGTIN"" T0 where T0.""Status""='O' and T0.""U_type""='PO'"
                    If CardCode <> "" Then strSQL += vbCrLf + " and T0.""U_partyid""='" & CardCode & "'"
                Else
                    strSQL = "Select T0.DocEntry,T0.DocNum,(Select Location from OLCT where Code=T0.U_loc) Location,T0.U_type Type,T0.U_partyid Party Id,T0.U_partynm Party Name,"
                    strSQL += vbCrLf + " T0.U_docdate Date,(Select firstName +' '+ lastName from OHEM where empID=T0.U_sname) Security Name,"
                    strSQL += vbCrLf + " T0.U_vehno Vehicle No.,T0.U_transnm Transporter Name"
                    strSQL += vbCrLf + " from [@MIGTIN] T0 where T0.Status='O' and T0.U_type='PO'"
                    If CardCode <> "" Then strSQL += vbCrLf + " and T0.U_partyid='" & CardCode & "'"
                End If

            Else 'from GE To GRPO
                If objAddOn.HANA Then
                    strSQL = "Select * from (Select T0.""DocEntry"" ,T0.""DocNum"",(Select ""Location"" from OLCT where ""Code""=T0.""U_loc"") ""Location"",T0.""U_type"" ""Type"",T0.""U_partynm"" ""Party Name"","
                    strSQL += vbCrLf + "T0.""U_docdate"" ""Date"",T1.""U_basenum"" ""PO Num"",T1.""U_basentry"" ""PO Entry"",T1.""U_baseline"" ""PO Line"",T1.""U_orderqty"" ""PO Qty"",T1.""U_itemcode"" ""ItemCode"",T1.""U_itemdesc"" ""ItemName"","
                    strSQL += vbCrLf + "T1.""U_qty"" -ifnull((Select sum(B.""U_Qty"") from ""@GEGRPO"" A join ""@GEGRPO1"" B on A.""DocEntry""=B.""DocEntry"""
                    strSQL += vbCrLf + "where A.""Canceled""='N' and B.""U_GEEntry""=T0.""DocEntry"" and B.""U_PoEntry""=T1.""U_basentry"" and B.""U_PoLine""=T1.""U_baseline"" and B.""U_ItemCode""=T1.""U_itemcode"" ),0)""Open Qty""," 'and A.""U_GRPOEntry"" is not null
                    strSQL += vbCrLf + "(Select ""firstName"" ||' '|| ""lastName"" from OHEM where cast(""empID"" as varchar)=T0.""U_sname"") ""Security Name"","
                    strSQL += vbCrLf + "T0.""U_vehno"" ""Vehicle No."",T0.""U_transnm"" ""Transporter Name"""
                    strSQL += vbCrLf + "from ""@MIGTIN"" T0 join ""@MIGTIN1"" T1 on T0.""DocEntry""=T1.""DocEntry"" left join POR1 T2 On T1.""U_basentry""=T2.""DocEntry"" and T1.""U_itemcode""=T2.""ItemCode"" and T1.""U_baseline""=T2.""LineNum"" where T0.""Status""='O' "
                    strSQL += vbCrLf + "and T1.""U_Linestat""='O' and T2.""LineStatus""='O' and T0.""U_type""='PO'" 'and T0.""U_Prostat""='0' 
                    'strSQL += vbCrLf + "and T1.""U_basentry"" not in (Select B.""U_PoEntry"" from ""@GEGRPO"" A join ""@GEGRPO1"" B on A.""DocEntry""=B.""DocEntry"""
                    'strSQL += vbCrLf + "where T1.""U_basentry""=B.""U_PoEntry"" and T1.""U_baseline""=B.""U_PoLine"" and A.""U_GRPOEntry"" is not null) "

                    ''join POR1 T2 On T1."U_basentry"=T2."DocEntry" and T1."U_itemcode"=T2."ItemCode" and T2."LineStatus"='O'
                    If CardCode <> "" Then strSQL += vbCrLf + " and T0.""U_partyid""='" & CardCode & "'"
                    If GEEntry <> "" Then strSQL += vbCrLf + " and T0.""DocEntry""='" & GEEntry & "'"
                    strSQL += vbCrLf + ") A where A.""Open Qty"">0 "
                    If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '%" & LikeText & "%' or UPPER(A.""" & LikeFieldName & """) like '" & LikeText & "') "
                    strSQL += vbCrLf + "Order by A.""DocEntry"" "
                Else
                    strSQL = "Select * from (Select T0.DocEntry ,T0.DocNum,(Select Location from OLCT where Code=T0.U_loc) Location,T0.U_type Type,T0.U_partynm [Party Name],"
                    strSQL += vbCrLf + "T0.U_docdate Date,T1.U_basenum [PO Num],T1.U_basentry [PO Entry],T1.U_baseline [PO Line],T1.U_orderqty [PO Qty],T1.U_itemcode ItemCode,T1.U_itemdesc ItemName,"
                    strSQL += vbCrLf + "T1.U_qty -ifnull((Select sum(B.U_Qty) from [@GEGRPO] A join [@GEGRPO1] B on A.DocEntry=B.DocEntry"
                    strSQL += vbCrLf + "where A.Canceled='N' and B.U_GEEntry=T0.DocEntry and B.U_PoEntry=T1.U_basentry and B.U_PoLine=T1.U_baseline and B.U_ItemCode=T1.U_itemcode ),0) [Open Qty]," 'and A.U_GRPOEntry is not null
                    strSQL += vbCrLf + "(Select firstName +' '+ lastName from OHEM where Cast(empID as varchar)=T0.U_sname) [Security Name],"
                    strSQL += vbCrLf + "T0.U_vehno [Vehicle No.],T0.U_transnm [Transporter Name]"
                    strSQL += vbCrLf + "from [@MIGTIN] T0 join [@MIGTIN1] T1 on T0.DocEntry=T1.DocEntry left join POR1 T2 On T1.U_basentry=T2.DocEntry and T1.U_itemcode=T2.ItemCode and T1.U_baseline=T2.LineNum where T0.Status='O' "
                    strSQL += vbCrLf + "and T1.U_Linestat='O' and T2.LineStatus='O' and T0.U_type='PO'" 'and T0.U_Prostat='0' 
                    'strSQL += vbCrLf + "and T1.U_basentry not in (Select B.U_PoEntry from [@GEGRPO A join [@GEGRPO1 B on A.DocEntry=B.DocEntry"
                    'strSQL += vbCrLf + "where T1.U_basentry=B.U_PoEntry and T1.U_baseline=B.U_PoLine and A.U_GRPOEntry is not null) "

                    ''join POR1 T2 On T1."U_basentry"=T2."DocEntry" and T1."U_itemcode"=T2."ItemCode" and T2."LineStatus"='O'
                    If CardCode <> "" Then strSQL += vbCrLf + " and T0.U_partyid='" & CardCode & "'"
                    If GEEntry <> "" Then strSQL += vbCrLf + " and T0.DocEntry='" & GEEntry & "'"
                    strSQL += vbCrLf + ") A where A.[Open Qty]>0 "
                    If LikeText <> "" Then strSQL += vbCrLf + "and (UPPER(B." & LikeFieldName & ") like '" & LikeText & "%' or UPPER(B." & LikeFieldName & ") like '%" & LikeText & "%' or UPPER(B." & LikeFieldName & ") like '" & LikeText & "') "
                    strSQL += vbCrLf + " Order by A.DocEntry "
                End If

            End If
            '" ,Case when T0.""U_vehstat""='1' then 'Entry' Else 'Exit' End ""Veh status"",Cast(TO_TIME(T0.""U_intime"") as varchar) ""Vehicle InTime""" &

            objForm.DataSources.DataTables.Item("Items").ExecuteQuery(strSQL)
            objGrid.DataTable = objForm.DataSources.DataTables.Item("Items")
            objGrid.RowHeaders.TitleObject.Caption = "#"

            For i As Integer = 0 To objGrid.Columns.Count - 1
                objGrid.Columns.Item(i).Editable = False
                objGrid.Columns.Item(i).TitleObject.Sortable = True
            Next
            For i As Integer = 0 To objGrid.Rows.Count - 1
                objGrid.RowHeaders.SetText(i, i + 1)
            Next
            Dim col As SAPbouiCOM.EditTextColumn
            If GRPO_Yes_No = "Y" Then
                col = objGrid.Columns.Item(1)
                col.LinkedObjectType = "MIGTIN"
            Else
                col = objGrid.Columns.Item(7)
                col.LinkedObjectType = 22
                col = objGrid.Columns.Item(1)
                col.LinkedObjectType = "MIGTIN"
            End If

        Catch ex As Exception

        End Try
    End Sub

End Class

