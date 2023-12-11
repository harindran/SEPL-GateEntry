Imports System.IO
Imports SAPbouiCOM.Framework
Imports SAPbobsCOM

Public Class clsAddOn
    Public WithEvents objApplication As SAPbouiCOM.Application
    Public objCompany As SAPbobsCOM.Company
    Dim oProgBarx As SAPbouiCOM.ProgressBar
    Public objGenFunc As Mukesh.SBOLib.GeneralFunctions
    Public objUIXml As Mukesh.SBOLib.UIXML
    Public ZB_row As Integer = 0
    Public SOMenuID As String = "0"

    Dim oUserObjectMD As SAPbobsCOM.UserObjectsMD
    Dim oUserTablesMD As SAPbobsCOM.UserTablesMD
    Dim ret As Long
    Dim strVal As String
    Dim objForm, objUDFForm As SAPbouiCOM.Form
    Dim MenuCount As Integer = 0
    Public objInward As clsGateInward
    Public objOutward As clsGateOutward
    Public objItemDetails As clsItemDetails
    Public objGEToGRPO As clsGEToGRPO
    Public objGRN As clsGRN
    Public HANA As Boolean = True
    'Public HANA As Boolean = False

    Public HWKEY() As String = New String() {"X1211807750", "L1653539483", "A1836445156", "M0394249985", "E0154677852", "F0123559701", "L1552968038", "M0090876837", "H0922924113"}

    Private Sub CheckLicense()

    End Sub

    Function isValidLicense() As Boolean
        Try
            objApplication.Menus.Item("257").Activate()
            Dim CrrHWKEY As String = objApplication.Forms.ActiveForm.Items.Item("79").Specific.Value.ToString.Trim
            objApplication.Forms.ActiveForm.Close()

            For i As Integer = 0 To HWKEY.Length - 1
                If HWKEY(i).Trim = CrrHWKEY.Trim Then
                    Return True
                End If
            Next
            MsgBox("Add-on installation failed due to license mismatch", MsgBoxStyle.OkOnly, "License Management")
            Return False
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        Return True
    End Function

    Public Sub Intialize()
        Dim objSBOConnector As New Mukesh.SBOLib.SBOConnector
        objApplication = objSBOConnector.GetApplication(System.Environment.GetCommandLineArgs.GetValue(1))
        objCompany = objSBOConnector.GetCompany(objApplication)
        Try
            createTables()
            createUDOs()
            createObjects()
            loadMenu()
        Catch ex As Exception
            objAddOn.objApplication.MessageBox(ex.Message)
            End
        End Try
        If isValidLicense() Then
            objApplication.SetStatusBarMessage("Addon connected successfully!", SAPbouiCOM.BoMessageTime.bmt_Short, False)
        Else
            objApplication.SetStatusBarMessage("Failed To Connect, Please Check The License Configuration", SAPbouiCOM.BoMessageTime.bmt_Short, False)
            objCompany.Disconnect()
            objApplication = Nothing
            objCompany = Nothing
            End
        End If
    End Sub

    Public Sub Intialize(ByVal args() As String)
        Try
            Dim oapplication As Application
            If (args.Length < 1) Then oapplication = New Application Else oapplication = New Application(args(0))
            objApplication = Application.SBO_Application
            If isValidLicense() Then
                objApplication.StatusBar.SetText("Establishing Company Connection Please Wait...", SAPbouiCOM.BoMessageTime.bmt_Long, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
                objCompany = Application.SBO_Application.Company.GetDICompany()
                Try
                    createObjects()
                    createTables()
                    createUDOs()
                    loadMenu()
                    If objAddOn.HANA Then
                        strVal = "SELECT ""MltpBrnchs"" FROM OADM"
                    Else
                        strVal = "SELECT MltpBrnchs FROM OADM"
                    End If
                    BranchFlag = objAddOn.objGenFunc.getSingleValue(strVal)
                    addReport_Layouttype("Gate Inward", "Gate Entry")
                    addReport_Layouttype("Gate Outward", "Gate Entry")
                    Add_Authorizations() 'User Permissions
                Catch ex As Exception
                    objAddOn.objApplication.MessageBox(ex.Message)
                    End
                End Try
                objApplication.StatusBar.SetText("Addon Connected Successfully..!!!", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
                oapplication.Run()
            Else
                objApplication.StatusBar.SetText("Addon Disconnected due to license mismatch..!!!", SAPbouiCOM.BoMessageTime.bmt_Medium, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                Exit Sub
            End If
            'System.Windows.Forms.Application.Run()
        Catch ex As Exception
            objAddOn.objApplication.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, True)
        End Try
    End Sub

    Private Sub createUDOs()
        Dim objUDFEngine As New Mukesh.SBOLib.UDFEngine(objCompany)
        Dim ct1(1) As String
        'ct1(0) = "" -----------Need to check -------------------------
        'objUDFEngine.createUDO("MIVHTYPE", "MIVHTYPE", "VehicleType", ct1, SAPbobsCOM.BoUDOObjType.boud_MasterData, False, False)
        ct1(0) = "MIGTOT1"
        ct1(1) = "MIGTOT2"
        objUDFEngine.createUDO("MIGTOT", "MIGTOT", "GTOutward", ct1, SAPbobsCOM.BoUDOObjType.boud_Document, False, True)
        ct1(0) = "MIGTIN1"
        ct1(1) = "MIGTIN2"
        objUDFEngine.createUDO("MIGTIN", "MIGTIN", "GTInward", ct1, SAPbobsCOM.BoUDOObjType.boud_Document, False, True)

    End Sub

    Private Sub createObjects()
        objGenFunc = New Mukesh.SBOLib.GeneralFunctions(objCompany)
        objUIXml = New Mukesh.SBOLib.UIXML(objApplication)
        objOutward = New clsGateOutward
        objInward = New clsGateInward
        objGRN = New clsGRN
        objItemDetails = New clsItemDetails
        objGEToGRPO = New clsGEToGRPO
    End Sub

    Public Sub Add_Authorizations()
        Try

            objAddOn.objGenFunc.AddToPermissionTree("Altrocks Tech", "ATPL_ADD-ON", "", "", "Y"c) 'Level 1 - Company Name

            objAddOn.objGenFunc.AddToPermissionTree("Gate Entry", "ATPL_GE", "", "ATPL_ADD-ON", "Y"c) 'Level 2 - Add-on Name

            objAddOn.objGenFunc.AddToPermissionTree("Gate Entry Inward", "ATPL_GI", "MIGTIN", "ATPL_GE", "Y"c) 'SubLevel of Level 2 - Screen Name
            objAddOn.objGenFunc.AddToPermissionTree("Gate Entry Outward", "ATPL_GO", "MIGTOT", "ATPL_GE", "Y"c) 'SubLevel of Level 2 - Screen Name


        Catch ex As Exception
        End Try
    End Sub

    Private Sub objApplication_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles objApplication.ItemEvent
        Try
            Dim objGrid As SAPbouiCOM.Grid
            Select Case pVal.FormTypeEx
                Case clsGateOutward.Formtype
                    objOutward.ItemEvent(FormUID, pVal, BubbleEvent)
                Case clsGateInward.Formtype
                    objInward.ItemEvent(FormUID, pVal, BubbleEvent)
                Case clsItemDetails.Formtype
                    objItemDetails.ItemEvent(FormUID, pVal, BubbleEvent)
                Case clsGRN.formtype
                    objGRN.ItemEvent(FormUID, pVal, BubbleEvent)
                Case clsGEToGRPO.Formtype
                    objGEToGRPO.ItemEvent(FormUID, pVal, BubbleEvent)
            End Select
            objForm = objAddOn.objApplication.Forms.Item(FormUID)
            Select Case pVal.EventType
                Case SAPbouiCOM.BoEventTypes.et_FORM_LOAD
                    If pVal.BeforeAction = False And pVal.FormTypeEx = "-143" Then
                        Dim objlink As SAPbouiCOM.LinkedButton
                        Dim objItem As SAPbouiCOM.Item

                        objItem = objForm.Items.Add("lnkgegrp", SAPbouiCOM.BoFormItemTypes.it_LINKED_BUTTON)
                        objItem.Left = objForm.Items.Item("U_GEGR").Left - 15
                        objItem.Width = 12
                        objItem.Top = objForm.Items.Item("U_GEGR").Top + 2
                        objItem.Height = 10
                        objlink = objItem.Specific
                        objlink.LinkedObjectType = "GEGRP"
                        objlink.Item.LinkTo = "U_GEGR"
                        objForm.Items.Item("U_GEGR").Enabled = False
                    End If
                Case SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED
                    If pVal.BeforeAction = True And pVal.FormTypeEx = "-143" And pVal.ItemUID = "lnkgegrp" Then
                        strVal = objForm.Items.Item("U_GEGR").Specific.String
                    End If
                    If pVal.BeforeAction = False And pVal.FormTypeEx = "-143" And pVal.ItemUID = "lnkgegrp" Then
                        Dim oGEForm As SAPbouiCOM.Form
                        Try
                            objAddOn.objApplication.Menus.Item("GEGRPO").Activate()
                            oGEForm = objAddOn.objApplication.Forms.ActiveForm
                            oGEForm.Freeze(True)
                            oGEForm.Mode = SAPbouiCOM.BoFormMode.fm_FIND_MODE
                            oGEForm.Items.Item("11").Enabled = True
                            Dim strsql As String
                            If objAddOn.HANA Then
                                strsql = objAddOn.objGenFunc.getSingleValue("Select ""DocNum"" from ""@GEGRPO"" where ""DocEntry""=" & strVal & " ") '" & objForm.Items.Item("U_GEGR").Specific.String & "
                            Else
                                strsql = objAddOn.objGenFunc.getSingleValue("Select ""DocNum"" from [@GEGRPO] where DocEntry=" & strVal & " ") '" & objForm.Items.Item("U_GEGR").Specific.String & "
                            End If
                            oGEForm.Items.Item("11").Specific.String = strsql
                            If objAddOn.HANA Then
                                strsql = objAddOn.objGenFunc.getSingleValue("Select To_VARCHAR(""U_DocDate"",'yyyyMMdd') from ""@GEGRPO"" where ""DocEntry""=" & strVal & " ") '" & objForm.Items.Item("U_GEGR").Specific.String & "
                            Else
                                strsql = objAddOn.objGenFunc.getSingleValue("Select Format(U_DocDate,'yyyyMMdd') from [@GEGRPO] where DocEntry=" & strVal & " ") '" & objForm.Items.Item("U_GEGR").Specific.String & "
                            End If

                            oGEForm.Items.Item("13").Specific.String = strsql
                            oGEForm.Items.Item("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular)
                            oGEForm.Mode = SAPbouiCOM.BoFormMode.fm_VIEW_MODE
                            Dim objGEmatrix As SAPbouiCOM.Matrix
                            objGEmatrix = oGEForm.Items.Item("8").Specific
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
                Case SAPbouiCOM.BoEventTypes.et_KEY_DOWN
                    If pVal.BeforeAction = True And pVal.FormTypeEx = "-143" And (pVal.ItemUID = "U_GEGR" Or pVal.ItemUID = "U_gever" Or pVal.ItemUID = "U_GateRem" Or pVal.ItemUID = "U_PartyId") Then
                        BubbleEvent = False
                    End If
                    If pVal.BeforeAction = True And (pVal.FormTypeEx = "-MIGTIN" Or pVal.FormTypeEx = "-MIGTOT" Or pVal.FormTypeEx = "-GEGRPO") Then BubbleEvent = False
                Case SAPbouiCOM.BoEventTypes.et_MATRIX_LINK_PRESSED
                    If pVal.BeforeAction = True And (pVal.FormTypeEx = "60004") Then
                        objGrid = objForm.Items.Item("Grid").Specific
                        strVal = objGrid.DataTable.GetValue("ObjType", objGrid.GetDataTableRowIndex(pVal.Row))
                        'strVal = objGrid.DataTable.GetValue("ObjType", objGrid.GetDataTableRowIndex(objGrid.Rows.SelectedRows.Item(0, SAPbouiCOM.BoOrderType.ot_SelectionOrder)))
                        Dim col As SAPbouiCOM.EditTextColumn
                        col = objGrid.Columns.Item(0)
                        col.LinkedObjectType = strVal
                    End If
                Case SAPbouiCOM.BoEventTypes.et_CLICK
                    If pVal.BeforeAction = False And (pVal.FormTypeEx = "60004") Then
                        objGrid = objForm.Items.Item("Grid").Specific
                        If objGrid.Rows.IsSelected(pVal.Row) = True Then
                            objGrid.Rows.SelectedRows.Remove(pVal.Row)
                        Else
                            objGrid.Rows.SelectedRows.Add(pVal.Row)
                        End If
                    End If
                    'If pVal.BeforeAction = True And bModal And (objAddOn.objApplication.Forms.ActiveForm.TypeEx = "MIGTIN" Or objAddOn.objApplication.Forms.ActiveForm.TypeEx = "MIGTOT") Then
                    '    objApplication.Forms.Item("ItemDetails").Select()
                    '    BubbleEvent = False
                    'End If
            End Select
        Catch ex As Exception
            'objAddOn.objApplication.MessageBox(ex.ToString, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_None)
        End Try
    End Sub

    Private Sub objApplication_FormDataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean) Handles objApplication.FormDataEvent
        Try
            Dim TranEntry As String = ""
            Select Case BusinessObjectInfo.FormTypeEx
                Case clsGateOutward.Formtype
                    objOutward.FormDataEvent(BusinessObjectInfo, BubbleEvent)
                Case clsGateInward.Formtype
                    objInward.FormDataEvent(BusinessObjectInfo, BubbleEvent)
                Case clsGRN.formtype
                    objGRN.FormDataEvent(BusinessObjectInfo, BubbleEvent)
                Case clsGEToGRPO.Formtype
                    objGEToGRPO.FormDataEvent(BusinessObjectInfo, BubbleEvent)
                Case "179", "180", "721", "940", "141"
                    If BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD And BusinessObjectInfo.ActionSuccess = True Then
                        objForm = objAddOn.objApplication.Forms.Item(BusinessObjectInfo.FormUID)
                        objUDFForm = objAddOn.objApplication.Forms.Item(objForm.UDFFormUID)
                        If BusinessObjectInfo.FormTypeEx = "179" Then  'A/R Credit Memo
                            TranEntry = objForm.DataSources.DBDataSources.Item("ORIN").GetValue("DocEntry", 0)
                        ElseIf BusinessObjectInfo.FormTypeEx = "180" Then  'Return
                            TranEntry = objForm.DataSources.DBDataSources.Item("ORDN").GetValue("DocEntry", 0)
                        ElseIf BusinessObjectInfo.FormTypeEx = "721" Then 'Goods Receipt
                            TranEntry = objForm.DataSources.DBDataSources.Item("OIGN").GetValue("DocEntry", 0)
                        End If
                        If objUDFForm.Items.Item("U_gever").Specific.String <> "" And TranEntry <> "" Then
                            If objAddOn.HANA Then
                                strVal = "Update ""@MIGTIN"" Set ""U_trgtentry""= " & TranEntry & " where ""DocEntry""='" & objUDFForm.Items.Item("U_gever").Specific.String & "'"
                            Else
                                strVal = "Update [@MIGTIN] Set U_trgtentry= " & TranEntry & " where DocEntry='" & objUDFForm.Items.Item("U_gever").Specific.String & "'"
                            End If
                            Dim objRS As SAPbobsCOM.Recordset
                            objRS = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            objRS.DoQuery(strVal)
                            If objAddOn.HANA Then
                                strVal = "Select ""TableName"" ""HTable"",Right(""TableName"",3)||'1' ""LTable"" from OBOB where ""ObjectId"" =" & BusinessObjectInfo.Type & ""
                            Else
                                strVal = "Select TableName HTable,Right(TableName,3) + '1' LTable from OBOB where ObjectId =" & BusinessObjectInfo.Type & ""
                            End If

                            objRS.DoQuery(strVal)
                            If objRS.RecordCount > 0 Then CloseGateEntry(BusinessObjectInfo.FormUID, objUDFForm.Items.Item("U_gever").Specific.String, objRS.Fields.Item("HTable").Value, objRS.Fields.Item("LTable").Value)
                        End If
                        'CloseGateEntry(BusinessObjectInfo.FormTypeEx)

                    End If
                Case "138", "228"
                    If BusinessObjectInfo.ActionSuccess = True And (BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD Or BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE) Then
                        objForm = objAddOn.objApplication.Forms.Item(BusinessObjectInfo.FormUID)
                        objUDFForm = objAddOn.objApplication.Forms.Item(objForm.UDFFormUID)
                        Dim selval As String = objUDFForm.Items.Item("U_GEIGRPD").Specific.Selected.Value
                        If strVal = selval Then Exit Sub
                        Remove_Menu({"43520,GT"}) : DisConnect_Addon()
                    Else
                        If objAddOn.HANA Then
                            strVal = objAddOn.objGenFunc.getSingleValue("Select ifnull(""U_GEIGRPD"",'Y')  from OADM")
                        Else
                            strVal = objAddOn.objGenFunc.getSingleValue("Select isnull(U_GEIGRPD,'Y') from OADM")
                        End If

                    End If
            End Select
        Catch ex As Exception

        End Try
        'If BusinessObjectInfo.BeforeAction Then
        'Else
        '    Try
        '    Catch ex As Exception
        '        'objAddOn.objApplication.StatusBar.SetText(ex.ToString, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success)
        '    End Try
        'End If
    End Sub

    Public Shared Function ErrorHandler(ByVal p_ex As Exception, ByVal objApplication As SAPbouiCOM.Application) As Boolean
        Dim sMsg As String = Nothing
        If p_ex.Message = "Form - already exists [66000-11]" Then
            Return True
            Exit Function  'ignore error
        End If
        Return False
    End Function

    Private Sub objApplication_MenuEvent(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean) Handles objApplication.MenuEvent
        Try
            If pVal.BeforeAction Then
            Else
                Select Case pVal.MenuUID
                    Case clsGateOutward.Formtype
                        objOutward.LoadScreen()
                    Case clsGateInward.Formtype
                        objInward.LoadScreen()
                    Case clsGEToGRPO.Formtype
                        objGEToGRPO.LoadScreen()
                End Select
            End If
            Try
                If objAddOn.objApplication.Forms.ActiveForm.UniqueID.Contains(clsGateOutward.Formtype) Then
                    objOutward.MenuEvent(pVal, BubbleEvent)
                ElseIf objAddOn.objApplication.Forms.ActiveForm.UniqueID.Contains(clsGateInward.Formtype) Then
                    objInward.MenuEvent(pVal, BubbleEvent)
                ElseIf objAddOn.objApplication.Forms.ActiveForm.UniqueID.Contains(clsGEToGRPO.Formtype) Then
                    objGEToGRPO.MenuEvent(pVal, BubbleEvent)
                End If
            Catch ex As Exception

            End Try

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    Private Sub loadMenu()
        'If objApplication.Menus.Item("43520").SubMenus.Exists("GT") Then Return
        MenuCount = objApplication.Menus.Item("43520").SubMenus.Count

        CreateMenu(Windows.Forms.Application.StartupPath + "\GE.png", MenuCount + 1, "Gate Entry Module", SAPbouiCOM.BoMenuType.mt_POPUP, "GT", objApplication.Menus.Item("43520"))
        CreateMenu("", 1, "Gate Entry Outward", SAPbouiCOM.BoMenuType.mt_STRING, clsGateOutward.Formtype, objApplication.Menus.Item("GT"))
        CreateMenu("", 2, "Gate Entry Inward", SAPbouiCOM.BoMenuType.mt_STRING, clsGateInward.Formtype, objApplication.Menus.Item("GT"))
        If objAddOn.HANA Then
            GE_Inward_GRPO_Draft = objGenFunc.getSingleValue("Select ifnull(""U_GEIGRPD"",'Y')  from OADM")
        Else
            GE_Inward_GRPO_Draft = objGenFunc.getSingleValue("Select isnull(U_GEIGRPD,'Y') from OADM")
        End If
        If GE_Inward_GRPO_Draft = "N" Then
            CreateMenu("", 3, "Gate Entry GRPO", SAPbouiCOM.BoMenuType.mt_STRING, clsGEToGRPO.Formtype, objApplication.Menus.Item("GT"))
        Else
            If objApplication.Menus.Item("GT").SubMenus.Exists(clsGEToGRPO.Formtype.ToString) Then objApplication.Menus.Item("GT").SubMenus.RemoveEx(clsGEToGRPO.Formtype.ToString)
        End If

    End Sub

    Public Function CreateMenu(ByVal ImagePath As String, ByVal Position As Int32, ByVal DisplayName As String, ByVal MenuType As SAPbouiCOM.BoMenuType, ByVal UniqueID As String, ByVal ParentMenu As SAPbouiCOM.MenuItem) As SAPbouiCOM.MenuItem
        Try
            Dim oMenuPackage As SAPbouiCOM.MenuCreationParams
            If ParentMenu.SubMenus.Exists(UniqueID.ToString) Then ParentMenu.SubMenus.RemoveEx(UniqueID.ToString)
            oMenuPackage = objApplication.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_MenuCreationParams)
            oMenuPackage.Image = ImagePath
            oMenuPackage.Position = Position
            oMenuPackage.Type = MenuType
            oMenuPackage.UniqueID = UniqueID
            oMenuPackage.String = DisplayName
            ParentMenu.SubMenus.AddEx(oMenuPackage)
        Catch ex As Exception
            objApplication.StatusBar.SetText("Menu Already Exists", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_None)
        End Try
        Return ParentMenu.SubMenus.Item(UniqueID)
    End Function

    Private Sub createTables()
        Dim objUDFEngine As New Mukesh.SBOLib.UDFEngine(objCompany)
        objAddOn.objApplication.SetStatusBarMessage("Creating Tables Please Wait...", SAPbouiCOM.BoMessageTime.bmt_Long, False)
        ' WriteSMSLog("0")
        'Gate Entry outward

        objUDFEngine.CreateTable("MIGTOT", "GTEntry Outward", SAPbobsCOM.BoUTBTableType.bott_Document)
        objUDFEngine.AddAlphaField("@MIGTOT", "loc", "Location", 15)
        objUDFEngine.AddAlphaField("@MIGTOT", "sname", "Security Name", 15)
        objUDFEngine.AddAlphaField("@MIGTOT", "type", "Type", 15)
        objUDFEngine.AddAlphaField("@MIGTOT", "partyid", "Party Id", 15)
        objUDFEngine.AddAlphaField("@MIGTOT", "partynm", "Party Name", 100)
        objUDFEngine.AddNumericField("@MIGTOT", "nopack", "No of Packages", 10)
        objUDFEngine.AddDateField("@MIGTOT", "outtime", "Out Time", SAPbobsCOM.BoFldSubTypes.st_Time)
        objUDFEngine.AddAlphaField("@MIGTOT", "lrno", "LRNo", 15)
        objUDFEngine.AddDateField("@MIGTOT", "docdate", "DocDate", SAPbobsCOM.BoFldSubTypes.st_None)
        ' objUDFEngine.AddAlphaField("@MIGTOT", "vehtype", "Vehicle Type", 15)
        objUDFEngine.AddAlphaField("@MIGTOT", "vehno", "Vehicle No", 15)
        '  objUDFEngine.AddAlphaField("@MIGTOT", "vehname", "Vehicle Name", 50)
        objUDFEngine.AddAlphaField("@MIGTOT", "transnm", "Transporter Name", 15)
        objUDFEngine.AddDateField("@MIGTOT", "lrdate", "LR Date", SAPbobsCOM.BoFldSubTypes.st_None)
        objUDFEngine.AddAlphaField("@MIGTOT", "geno", "Gate Entry No", 25)
        objUDFEngine.AddAlphaField("@MIGTOT", "wcno", "Weight Challan No", 25)
        objUDFEngine.AddDateField("@MIGTOT", "cutdate", "Cutoff Date", SAPbobsCOM.BoFldSubTypes.st_None)
        objUDFEngine.AddDateField("@MIGTOT", "intime", "In Time", SAPbobsCOM.BoFldSubTypes.st_Time)
        objUDFEngine.AddAlphaField("@MIGTOT", "vehstat", "Vehicle Status", 10, "1,2", "Entry,Exit", "1")
        objUDFEngine.AddAlphaField("@MIGTOT", "TranType", "Transaction Type", 30)
        objUDFEngine.AddAlphaField("@MIGTOT", "Branch", "Branch ID", 15)
        objUDFEngine.AddAlphaField("@MIGTOT", "DriverName", "Driver Name", 100)

        objUDFEngine.CreateTable("MIGTOT1", "GT Outward Line", SAPbobsCOM.BoUTBTableType.bott_DocumentLines)
        objUDFEngine.AddAlphaField("@MIGTOT1", "basenum", "Base Document Number", 15)
        objUDFEngine.AddAlphaField("@MIGTOT1", "baseline", "Base Line", 15)
        objUDFEngine.AddAlphaField("@MIGTOT1", "basentry", "Base Entry", 15)
        objUDFEngine.AddAlphaField("@MIGTOT1", "basetype", "Base Type", 15)
        objUDFEngine.AddAlphaField("@MIGTOT1", "itemcode", "Item/Service Code", 50)
        objUDFEngine.AddAlphaField("@MIGTOT1", "itemdesc", "Item/Service Description", 100)
        'objUDFEngine.AddAlphaField("@MIGTOT1", "itemdet", "Item/Service Details", 254)
        '-------------------- Need to add text field --------------------------------------------------
        objUDFEngine.AddFloatField("@MIGTOT1", "qty", "Quantity", SAPbobsCOM.BoFldSubTypes.st_Quantity)
        objUDFEngine.AddFloatField("@MIGTOT1", "unitpric", "Unit Price", SAPbobsCOM.BoFldSubTypes.st_Price)
        objUDFEngine.AddFloatField("@MIGTOT1", "linetot", "Line Total", SAPbobsCOM.BoFldSubTypes.st_Sum)
        objUDFEngine.AddFloatField("@MIGTOT1", "orderqty", "Order Quantity", SAPbobsCOM.BoFldSubTypes.st_Quantity)
        objUDFEngine.AddFloatField("@MIGTOT1", "pendqty", "Pending Quantity", SAPbobsCOM.BoFldSubTypes.st_Quantity)
        objUDFEngine.AddFloatField("@MIGTOT1", "gateqty", "Gate Quantity", SAPbobsCOM.BoFldSubTypes.st_Quantity)
        objUDFEngine.AddAlphaField("@MIGTOT1", "remarks", "Remarks ", 254)
        objUDFEngine.AddAlphaField("@MIGTOT1", "uom", "UoM ", 10)
        objUDFEngine.AddAlphaMemoField("@MIGTOT1", "itemdet1", "ItemDetails", 4)
        objUDFEngine.AddAlphaField("@MIGTOT1", "Linestat", "Line Status", 10, "O,C", "Open,Close", "O")

        objUDFEngine.CreateTable("MIGTOT2", "GT Inward Line 2", SAPbobsCOM.BoUTBTableType.bott_DocumentLines)
        objUDFEngine.AddAlphaMemoField("@MIGTOT2", "trgtPath", "Target Path", 200)
        objUDFEngine.AddAlphaMemoField("@MIGTOT2", "SrcPath", "Source Path", 200)
        objUDFEngine.AddDateField("@MIGTOT2", "Date", "Attach Date", SAPbobsCOM.BoFldSubTypes.st_None)
        objUDFEngine.AddAlphaField("@MIGTOT2", "FileName", "File Name", 30)
        objUDFEngine.AddAlphaField("@MIGTOT2", "FileExt", "File Extension", 30)
        objUDFEngine.AddAlphaField("@MIGTOT2", "FreeText", "Free Text", 100)


        'Gate Entry Inward

        objUDFEngine.CreateTable("MIGTIN", "GTEntry Inward", SAPbobsCOM.BoUTBTableType.bott_Document)
        objUDFEngine.AddAlphaField("@MIGTIN", "loc", "Location", 15)
        'objUDFEngine.AddAlphaField("@MIGTIN", "type", "Type", 30, "PO,SR,DR,MI", "Purchase Order,Sales Return,Delivery,Material Inward", "PO")
        objUDFEngine.AddAlphaField("@MIGTIN", "type", "Type", 15)
        objUDFEngine.AddAlphaField("@MIGTIN", "partyid", "Party Id", 25)
        objUDFEngine.AddAlphaField("@MIGTIN", "partynm", "Party Name", 100)
        objUDFEngine.AddAlphaField("@MIGTIN", "supdcno", "Supplier DC No", 15)
        objUDFEngine.AddAlphaField("@MIGTIN", "supinvno", "Supplier InvNo", 15)
        objUDFEngine.AddAlphaField("@MIGTIN", "mdvtcprd", "ModVatCopy Received", 5)
        objUDFEngine.AddNumericField("@MIGTIN", "nopack", "No of Packages", 10)
        objUDFEngine.AddDateField("@MIGTIN", "intime", "Out Time", SAPbobsCOM.BoFldSubTypes.st_Time)
        objUDFEngine.AddAlphaField("@MIGTIN", "lrno", "LR No", 15)
        objUDFEngine.AddDateField("@MIGTIN", "lrdate", "LR Date", SAPbobsCOM.BoFldSubTypes.st_None)
        objUDFEngine.AddAlphaField("@MIGTIN", "sname", "Security Name", 15)
        objUDFEngine.AddDateField("@MIGTIN", "docdate", "DocDate", SAPbobsCOM.BoFldSubTypes.st_None)
        objUDFEngine.AddDateField("@MIGTIN", "supdcdt", "Supplier DC Date", SAPbobsCOM.BoFldSubTypes.st_None)
        objUDFEngine.AddDateField("@MIGTIN", "supinvdt", "Supplier InvDate", SAPbobsCOM.BoFldSubTypes.st_None)
        ' objUDFEngine.AddAlphaField("@MIGTIN", "vehtype", "Vehicle Type", 15)
        objUDFEngine.AddAlphaField("@MIGTIN", "vehno", "Vehicle No", 15)
        '  objUDFEngine.AddAlphaField("@MIGTIN", "vehname", "Vehicle Name", 50)
        objUDFEngine.AddAlphaField("@MIGTIN", "transnm", "Transporter Name", 50)
        objUDFEngine.AddAlphaField("@MIGTIN", "copyto", "Copy To", 15)
        objUDFEngine.AddAlphaField("@MIGTIN", "geno", "Gate Entry No", 25)
        objUDFEngine.AddAlphaField("@MIGTIN", "wcno", "Weight Challan No", 25)
        objUDFEngine.AddDateField("@MIGTIN", "cutdate", "Cutoff Date", SAPbobsCOM.BoFldSubTypes.st_None)
        objUDFEngine.AddDateField("@MIGTIN", "outtime", "Out Time", SAPbobsCOM.BoFldSubTypes.st_Time)
        objUDFEngine.AddAlphaField("@MIGTIN", "vehstat", "Vehicle Status", 10, "1,2", "Entry,Exit", "1")
        objUDFEngine.AddAlphaField("@MIGTIN", "trgtentry", "Target Entry", 15)
        objUDFEngine.AddAlphaField("@MIGTIN", "Prostat", "Process Status", 50)
        objUDFEngine.AddAlphaField("@MIGTIN", "Branch", "Branch ID", 15)
        objUDFEngine.AddAlphaField("@MIGTIN", "TranType", "Transaction Type", 30)
        objUDFEngine.AddAlphaField("@MIGTIN", "DriverName", "Driver Name", 100)


        objUDFEngine.CreateTable("MIGTIN1", "GT Inward Line", SAPbobsCOM.BoUTBTableType.bott_DocumentLines)
        objUDFEngine.AddAlphaField("@MIGTIN1", "basetype", "Base Type", 15)
        objUDFEngine.AddAlphaField("@MIGTIN1", "basenum", "Base Document Number", 15)
        objUDFEngine.AddAlphaField("@MIGTIN1", "baseline", "Base Line", 15)
        objUDFEngine.AddAlphaField("@MIGTIN1", "basentry", "Base Entry", 15)
        objUDFEngine.AddAlphaField("@MIGTIN1", "itemcode", "Item/Service Code", 50)
        objUDFEngine.AddAlphaField("@MIGTIN1", "itemdesc", "Item/Service Description", 100)
        '  objUDFEngine.AddAlphaField("@MIGTIN1", "itemdet", "Item/Service Details", 15)
        objUDFEngine.AddFloatField("@MIGTIN1", "qty", "Quantity", SAPbobsCOM.BoFldSubTypes.st_Quantity)
        objUDFEngine.AddFloatField("@MIGTIN1", "unitpric", "Unit Price", SAPbobsCOM.BoFldSubTypes.st_Price)
        objUDFEngine.AddFloatField("@MIGTIN1", "linetot", "Line Total", SAPbobsCOM.BoFldSubTypes.st_Sum)
        objUDFEngine.AddFloatField("@MIGTIN1", "orderqty", "Order Quantity", SAPbobsCOM.BoFldSubTypes.st_Quantity)
        objUDFEngine.AddFloatField("@MIGTIN1", "pendqty", "Pending Quantity", SAPbobsCOM.BoFldSubTypes.st_Quantity)
        objUDFEngine.AddFloatField("@MIGTIN1", "gateqty", "Gate Quantity", SAPbobsCOM.BoFldSubTypes.st_Quantity)
        objUDFEngine.AddAlphaField("@MIGTIN1", "remarks", "Remarks ", 100)
        objUDFEngine.AddAlphaField("@MIGTIN1", "uom", "UoM ", 10)
        objUDFEngine.AddAlphaMemoField("@MIGTIN1", "itemdet1", "ItemDetails", 4)
        objUDFEngine.AddAlphaField("@MIGTIN1", "Linestat", "Line Status", 10, "O,C", "Open,Close", "O")

        objUDFEngine.CreateTable("MIGTIN2", "GT Inward Line 2", SAPbobsCOM.BoUTBTableType.bott_DocumentLines)
        objUDFEngine.AddAlphaMemoField("@MIGTIN2", "trgtPath", "Target Path", 200)
        objUDFEngine.AddAlphaMemoField("@MIGTIN2", "SrcPath", "Source Path", 200)
        objUDFEngine.AddDateField("@MIGTIN2", "Date", "Attach Date", SAPbobsCOM.BoFldSubTypes.st_None)
        objUDFEngine.AddAlphaField("@MIGTIN2", "FileName", "File Name", 30)
        objUDFEngine.AddAlphaField("@MIGTIN2", "FileExt", "File Extension", 30)
        objUDFEngine.AddAlphaField("@MIGTIN2", "FreeText", "Free Text", 100)


        ' vehicle type master
        objUDFEngine.CreateTable("MIVHTYPE", "Vehicle Type", SAPbobsCOM.BoUTBTableType.bott_MasterData)
        ' Marketting documents
        objUDFEngine.AddAlphaField("INV1", "getype", "GateEntry Type", 15)
        objUDFEngine.AddAlphaField("INV1", "geentry", "GateEntry DocEntry", 15)
        objUDFEngine.AddAlphaField("INV1", "gedocno", "GateEntry DocNum", 15)
        objUDFEngine.AddAlphaField("OWTR", "VENDORCODE", "Vendor Code", 30)
        objUDFEngine.AddAlphaField("OWTR", "VENDORNAME", "Vendor Name", 100)
        objUDFEngine.AddAlphaField("OPDN", "gever", "GE Verification", 15)

        objUDFEngine.AddAlphaField("OPDN", "TransTyp", "Transaction Type", 5, "REG,REJ,RDC,SHG,NRDC,GEN,SPOOL", "Regular,Rejection,Returnable DC,Shortage,Non-Returnable,General,SPOOL", "REG")

        objUDFEngine.AddFloatField("PDN1", "GateQty", "GateEntry Quantity", SAPbobsCOM.BoFldSubTypes.st_Quantity)
        objUDFEngine.AddFloatField("PDN1", "DiffQty", "GE Difference Qty", SAPbobsCOM.BoFldSubTypes.st_Quantity)

        objUDFEngine.AddAlphaField("OADM", "GEIGRPD", "GE Inward GRPO Draft", 3, "Y,N", "Yes,No", "N")
        Gate_Entry_To_GRPO_Invoice()
        objUDFEngine.AddAlphaMemoField("OPDN", "GateRem", "Gate Remarks", 100)
        objUDFEngine.AddAlphaMemoField("PDN1", "GateRem", "Gate Remarks", 100)
        objUDFEngine.AddAlphaField("OPDN", "PartyId", "Gate party ID", 15)
        '*******************  Table ******************* START********************************* END
    End Sub

    Private Sub Gate_Entry_To_GRPO_Invoice()
        Dim objUDFEngine As New Mukesh.SBOLib.UDFEngine(objCompany)
        objUDFEngine.CreateTable("GEGRPO", "Gate Entry To GRPO Header", SAPbobsCOM.BoUTBTableType.bott_Document)
        objUDFEngine.CreateTable("GEGRPO1", "Gate Entry To GRPO Lines", SAPbobsCOM.BoUTBTableType.bott_DocumentLines)

        objUDFEngine.AddAlphaField("@GEGRPO", "BPCode", "Vendor Code", 30)
        'objUDFEngine.AddAlphaField("@GEGRPO", "BPName", "Vendor Name", 100)
        objUDFEngine.AddDateField("@GEGRPO", "DocDate", "Document Date", SAPbobsCOM.BoFldSubTypes.st_None)
        objUDFEngine.AddAlphaField("@GEGRPO", "GEEntry", "GE Entry", 30)
        objUDFEngine.AddAlphaField("@GEGRPO", "GRPOEntry", "GRPO Entry", 30)

        objUDFEngine.AddAlphaField("@GEGRPO1", "GENum", "GE DocNum", 30)
        objUDFEngine.AddAlphaField("@GEGRPO1", "GEEntry", "GE DocEntry", 30)
        objUDFEngine.AddDateField("@GEGRPO1", "GEDate", "GE DocDate", SAPbobsCOM.BoFldSubTypes.st_None)
        objUDFEngine.AddAlphaField("@GEGRPO1", "ItemCode", "Item Code", 50)
        objUDFEngine.AddAlphaField("@GEGRPO1", "ItemName", "Item Name", 100)
        objUDFEngine.AddFloatField("@GEGRPO1", "GEQty", "GE Quantity", SAPbobsCOM.BoFldSubTypes.st_Quantity)
        objUDFEngine.AddAlphaField("@GEGRPO1", "PoEntry", "PO Entry", 30)
        objUDFEngine.AddAlphaField("@GEGRPO1", "PoLine", "PO Line", 30)
        objUDFEngine.AddDateField("@GEGRPO1", "PoDate", "PO Date", SAPbobsCOM.BoFldSubTypes.st_None)
        objUDFEngine.AddFloatField("@GEGRPO1", "GRPOQty", "GRPO Quantity", SAPbobsCOM.BoFldSubTypes.st_Quantity)
        objUDFEngine.AddAlphaField("@GEGRPO1", "Uom", "Uom", 10)
        objUDFEngine.AddFloatField("@GEGRPO1", "PoQty", "PO Quantity", SAPbobsCOM.BoFldSubTypes.st_Quantity)
        objUDFEngine.AddFloatField("@GEGRPO1", "Qty", "Quantity", SAPbobsCOM.BoFldSubTypes.st_Quantity)
        objUDFEngine.AddFloatField("@GEGRPO1", "OpenQty", "Open Quantity", SAPbobsCOM.BoFldSubTypes.st_Quantity)
        objUDFEngine.AddAlphaField("@GEGRPO1", "Whse", "Warehouse Code", 10)

        objUDFEngine.AddAlphaField("OPDN", "GEGR", "GE-GRPO Entry", 30)
        objUDFEngine.AddAlphaField("@MIGTIN", "GEGR", "GE-GRPO Entry", 30)
        objUDFEngine.AddAlphaField("@MIGTIN", "GRPOEntry", "GRPO Entry", 30)

        objUDFEngine.AddUDO("GEGRP", "GE To GRPO", SAPbobsCOM.BoUDOObjType.boud_Document, "GEGRPO", {"GEGRPO1"}, {"DocEntry", "DocNum", "U_BPCode", "U_DocDate"}, True, True)


    End Sub

    Private Sub objApplication_RightClickEvent(ByRef eventInfo As SAPbouiCOM.ContextMenuInfo, ByRef BubbleEvent As Boolean) Handles objApplication.RightClickEvent
        Try
            Select Case objAddOn.objApplication.Forms.ActiveForm.TypeEx
                Case clsGateInward.Formtype
                    objInward.RightClickEvent(eventInfo, BubbleEvent)
                Case clsGateOutward.Formtype
                    objOutward.RightClickEvent(eventInfo, BubbleEvent)
                Case clsGEToGRPO.Formtype
                    objGEToGRPO.RightClickEvent(eventInfo, BubbleEvent)
            End Select
        Catch ex As Exception
        End Try
    End Sub

    Private Sub applyFilter()
        Dim oFilters As SAPbouiCOM.EventFilters
        Dim oFilter As SAPbouiCOM.EventFilter
        oFilters = New SAPbouiCOM.EventFilters
        'Item Master Data 
        oFilter = oFilters.Add(SAPbouiCOM.BoEventTypes.et_GOT_FOCUS)

        oFilter = oFilters.Add(SAPbouiCOM.BoEventTypes.et_MENU_CLICK)

        oFilter = oFilters.Add(SAPbouiCOM.BoEventTypes.et_FORM_ACTIVATE)

        oFilter = oFilters.Add(SAPbouiCOM.BoEventTypes.et_RIGHT_CLICK)

    End Sub

    Public Sub WriteSMSLog(ByVal Str As String)
        Dim fs As FileStream
        Dim chatlog As String = Windows.Forms.Application.StartupPath & "\Log_" & Today.ToString("yyyyMMdd") & ".txt"
        If File.Exists(chatlog) Then
        Else
            fs = New FileStream(chatlog, FileMode.Create, FileAccess.Write)
            fs.Close()
        End If
        ' Dim objReader As New System.IO.StreamReader(chatlog)
        Dim sdate As String
        sdate = Now
        'objReader.Close()
        If System.IO.File.Exists(chatlog) = True Then
            Dim objWriter As New System.IO.StreamWriter(chatlog, True)
            objWriter.WriteLine(sdate & " : " & Str)
            objWriter.Close()
        Else
            Dim objWriter As New System.IO.StreamWriter(chatlog, False)
            ' MsgBox("Failed to send message!")
        End If
    End Sub

    Private Sub CloseGateEntry(ByVal FormUID As String, ByVal GEEntry As String, ByVal HTable As String, ByVal LTable As String)
        Try
            Dim strSQL As String
            Dim objRS As SAPbobsCOM.Recordset
            'Dim oForm As SAPbouiCOM.Form
            'oForm = objAddOn.objApplication.Forms.GetForm(FormUID, 1)
            'Dim oMatrix As SAPbouiCOM.Matrix
            'Dim MatrixNo As String
            'Select Case FormUID
            '    Case "179" 'ARCreditMemo
            '        MatrixNo = "38"
            '    Case "180" 'SalesReturn
            '        MatrixNo = "38"
            '    Case "721" ' Goods Receipt
            '        MatrixNo = "13"
            '    Case "940" 'Stock transfer
            '        MatrixNo = "23"
            '    Case "141" ' APInvoice
            '        MatrixNo = "39"
            'End Select
            'oMatrix = oForm.Items.Item(MatrixNo).Specific
            'Dim GEDocEntry As String = Trim(oMatrix.Columns.Item("U_geentry").Cells.Item(1).Specific.value)

            objForm = objAddOn.objApplication.Forms.Item(FormUID)
            objRS = objAddOn.objCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            If objAddOn.HANA Then
                strSQL = " Update T0 Set ""U_Linestat""= Case when (T0.""U_qty""-T1.""Qty"")<=0 then 'C' Else 'O' End from ""@MIGTIN1"" T0 "
                strSQL += vbCrLf + "inner join (Select ifnull(Sum(B.""Quantity""),0) ""Qty"",B.""BaseType"",B.""BaseEntry"",B.""BaseLine"",A.""U_gever"" from " & HTable & " A join " & LTable & " B on A.""DocEntry""=B.""DocEntry"""
                strSQL += vbCrLf + "where ""CANCELED"" ='N' group by B.""BaseType"",B.""BaseEntry"",B.""BaseLine"",A.""U_gever"") as T1"
                strSQL += vbCrLf + "on T1.""BaseType""=T0.""U_basetype"" and T1.""BaseEntry""=T0.""U_basentry"" and T1.""U_gever""=T0.""DocEntry"""
                strSQL += vbCrLf + "and T1.""BaseLine""=T0.""U_baseline"" where T0.""DocEntry""='" & GEEntry & "' and T0.""U_Linestat""='O' "
                objRS.DoQuery(strSQL)
                strSQL = "Update T0 Set ""Status""=Case when T1.""U_Linestat""='C' then 'C' Else 'O' End from ""@MIGTIN"" T0 inner join "
                strSQL += vbCrLf + "(Select Top 1 B.""U_Linestat"",B.""DocEntry"" from ""@MIGTIN1"" B where B.""DocEntry""='" & GEEntry & "' Order by B.""U_Linestat"" desc ) as T1"
                strSQL += vbCrLf + "on T1.""DocEntry""=T0.""DocEntry""  where T0.""DocEntry""='" & GEEntry & "' and T0.""Status""='O'"
                objRS.DoQuery(strSQL)
            Else
                strSQL = " Update T0 Set U_Linestat= Case when (T0.U_qty-T1.Qty)<=0 then 'C' Else 'O' End from [@MIGTIN1] T0 "
                strSQL += vbCrLf + "inner join (Select isnull(Sum(B.Quantity),0) Qty,B.BaseType,B.BaseEntry,B.BaseLine,A.U_gever from " & HTable & " A join " & LTable & " B on A.DocEntry=B.DocEntry"
                strSQL += vbCrLf + "where CANCELED ='N' group by B.BaseType,B.BaseEntry,B.BaseLine,A.U_gever) as T1"
                strSQL += vbCrLf + "on T1.BaseType=T0.U_basetype and T1.BaseEntry=T0.U_basentry and T1.U_gever=T0.DocEntry"
                strSQL += vbCrLf + "and T1.BaseLine=T0.U_baseline where T0.DocEntry='" & GEEntry & "' and T0.U_Linestat='O' "
                objRS.DoQuery(strSQL)
                strSQL = "Update T0 Set Status=Case when T1.U_Linestat='C' then 'C' Else 'O' End from [@MIGTIN] T0 inner join "
                strSQL += vbCrLf + "(Select Top 1 B.U_Linestat,B.DocEntry from [@MIGTIN1] B where B.DocEntry='" & GEEntry & "' Order by B.U_Linestat desc ) as T1"
                strSQL += vbCrLf + "on T1.DocEntry=T0.DocEntry  where T0.DocEntry='" & GEEntry & "' and T0.Status='O'"
                objRS.DoQuery(strSQL)
            End If

            objRS = Nothing
        Catch ex As Exception

        End Try

    End Sub

    Private Sub addJobCardReporttype()
        'Dim rptTypeService As SAPbobsCOM.ReportTypesService
        'Dim newType As SAPbobsCOM.ReportType
        'Dim newtypeParam As SAPbobsCOM.ReportTypeParams
        'Dim newReportParam As SAPbobsCOM.ReportLayoutParams
        'Dim ReportExists As Boolean = False
        'Try


        '    Dim newtypesParam As SAPbobsCOM.ReportTypesParams
        '    rptTypeService = objAddOn.objCompany.GetCompanyService.GetBusinessService(SAPbobsCOM.ServiceTypes.ReportTypesService)
        '    newtypesParam = rptTypeService.GetReportTypeList

        '    Dim i As Integer
        '    For i = 0 To newtypesParam.Count - 1
        '        If newtypesParam.Item(i).TypeName = clsJobCard.FormType And newtypesParam.Item(i).MenuID = clsJobCard.FormType Then
        '            ReportExists = True
        '            Exit For
        '        End If
        '    Next i

        '    If Not ReportExists Then
        '        rptTypeService = objCompany.GetCompanyService.GetBusinessService(SAPbobsCOM.ServiceTypes.ReportTypesService)
        '        newType = rptTypeService.GetDataInterface(SAPbobsCOM.ReportTypesServiceDataInterfaces.rtsReportType)


        '        newType.TypeName = clsJobCard.FormType
        '        newType.AddonName = "JC2Addon"
        '        newType.AddonFormType = clsJobCard.FormType
        '        newType.MenuID = clsJobCard.FormType
        '        newtypeParam = rptTypeService.AddReportType(newType)

        '        Dim rptService As SAPbobsCOM.ReportLayoutsService
        '        Dim newReport As SAPbobsCOM.ReportLayout
        '        rptService = objCompany.GetCompanyService.GetBusinessService(SAPbobsCOM.ServiceTypes.ReportLayoutsService)
        '        newReport = rptService.GetDataInterface(SAPbobsCOM.ReportLayoutsServiceDataInterfaces.rlsdiReportLayout)
        '        newReport.Author = objCompany.UserName
        '        newReport.Category = SAPbobsCOM.ReportLayoutCategoryEnum.rlcCrystal
        '        newReport.Name = clsJobCard.FormType
        '        newReport.TypeCode = newtypeParam.TypeCode

        '        newReportParam = rptService.AddReportLayout(newReport)

        '        newType = rptTypeService.GetReportType(newtypeParam)
        '        newType.DefaultReportLayout = newReportParam.LayoutCode
        '        rptTypeService.UpdateReportType(newType)

        '        Dim oBlobParams As SAPbobsCOM.BlobParams
        '        oBlobParams = objCompany.GetCompanyService.GetDataInterface(SAPbobsCOM.CompanyServiceDataInterfaces.csdiBlobParams)
        '        oBlobParams.Table = "RDOC"
        '        oBlobParams.Field = "Template"
        '        Dim oKeySegment As SAPbobsCOM.BlobTableKeySegment
        '        oKeySegment = oBlobParams.BlobTableKeySegments.Add
        '        oKeySegment.Name = "DocCode"
        '        oKeySegment.Value = newReportParam.LayoutCode

        '        Dim oFile As FileStream
        '        oFile = New FileStream(Application.StartupPath + "\JobCard.rpt", FileMode.Open)
        '        Dim fileSize As Integer
        '        fileSize = oFile.Length
        '        Dim buf(fileSize) As Byte
        '        oFile.Read(buf, 0, fileSize)
        '        oFile.Dispose()

        '        Dim oBlob As SAPbobsCOM.Blob
        '        oBlob = objCompany.GetCompanyService.GetDataInterface(SAPbobsCOM.CompanyServiceDataInterfaces.csdiBlob)
        '        oBlob.Content = Convert.ToBase64String(buf, 0, fileSize)
        '        objCompany.GetCompanyService.SetBlob(oBlobParams, oBlob)
        '    End If
        'Catch ex As Exception
        '    objApplication.MessageBox(ex.ToString)
        'End Try

    End Sub

    Public Sub addReport_Layouttype(ByVal FormType As String, ByVal AddonName As String)
        Dim rptTypeService As SAPbobsCOM.ReportTypesService
        Dim newType As SAPbobsCOM.ReportType
        Dim newtypeParam As SAPbobsCOM.ReportTypeParams
        Dim newReportParam As SAPbobsCOM.ReportLayoutParams
        Dim ReportExists As Boolean = False
        Dim strsql As String
        Try
            'For Changing add-on Layouts Name and Layout Menu ID 
            'update RTYP set Name='MCarriedOut'  where Name='CarriedOut'
            'update RDOC set DocName='MCarriedOut' where DocName='CarriedOut'
            Dim newtypesParam As SAPbobsCOM.ReportTypesParams
            rptTypeService = objAddOn.objCompany.GetCompanyService.GetBusinessService(SAPbobsCOM.ServiceTypes.ReportTypesService)
            newtypesParam = rptTypeService.GetReportTypeList
            If HANA Then
                strsql = objGenFunc.getSingleValue("Select 1 as ""Status"" from RTYP Where ""NAME""='" & FormType & "'")
            Else
                strsql = objGenFunc.getSingleValue("Select 1 as Status from RTYP Where NAME='" & FormType & "'")
            End If

            If strsql <> "" Then Exit Sub
            ReportExists = True
            'Dim i As Integer
            'For i = 0 To newtypesParam.Count - 1
            '    If newtypesParam.Item(i).TypeName = FormType And newtypesParam.Item(i).MenuID = FormType Then
            '        ReportExists = True
            '        Exit For
            '    End If
            'Next i

            If Not ReportExists Then
                rptTypeService = objAddOn.objCompany.GetCompanyService.GetBusinessService(SAPbobsCOM.ServiceTypes.ReportTypesService)
                newType = rptTypeService.GetDataInterface(SAPbobsCOM.ReportTypesServiceDataInterfaces.rtsReportType)

                newType.TypeName = FormType 'clsJobCard.FormType
                newType.AddonName = AddonName ' "Sub-Con Add-on"
                newType.AddonFormType = FormType
                newType.MenuID = FormType
                newtypeParam = rptTypeService.AddReportType(newType)

                Dim rptService As SAPbobsCOM.ReportLayoutsService
                Dim newReport As SAPbobsCOM.ReportLayout
                rptService = objAddOn.objCompany.GetCompanyService.GetBusinessService(SAPbobsCOM.ServiceTypes.ReportLayoutsService)
                newReport = rptService.GetDataInterface(SAPbobsCOM.ReportLayoutsServiceDataInterfaces.rlsdiReportLayout)
                newReport.Author = objAddOn.objCompany.UserName
                newReport.Category = SAPbobsCOM.ReportLayoutCategoryEnum.rlcCrystal
                newReport.Name = FormType
                newReport.TypeCode = newtypeParam.TypeCode

                newReportParam = rptService.AddReportLayout(newReport)

                newType = rptTypeService.GetReportType(newtypeParam)
                newType.DefaultReportLayout = newReportParam.LayoutCode
                rptTypeService.UpdateReportType(newType)

                Dim oBlobParams As SAPbobsCOM.BlobParams
                oBlobParams = objAddOn.objCompany.GetCompanyService.GetDataInterface(SAPbobsCOM.CompanyServiceDataInterfaces.csdiBlobParams)
                oBlobParams.Table = "RDOC"
                oBlobParams.Field = "Template"
                Dim oKeySegment As SAPbobsCOM.BlobTableKeySegment
                oKeySegment = oBlobParams.BlobTableKeySegments.Add
                oKeySegment.Name = "DocCode"
                oKeySegment.Value = newReportParam.LayoutCode

                Dim oFile As FileStream
                oFile = New FileStream(System.Windows.Forms.Application.StartupPath + "\Sample.rpt", FileMode.Open)
                Dim fileSize As Integer
                fileSize = oFile.Length
                Dim buf(fileSize) As Byte
                oFile.Read(buf, 0, fileSize)
                oFile.Dispose()

                Dim oBlob As SAPbobsCOM.Blob
                oBlob = objAddOn.objCompany.GetCompanyService.GetDataInterface(SAPbobsCOM.CompanyServiceDataInterfaces.csdiBlob)
                oBlob.Content = Convert.ToBase64String(buf, 0, fileSize)
                objAddOn.objCompany.GetCompanyService.SetBlob(oBlobParams, oBlob)
            End If
        Catch ex As Exception
            objAddOn.objApplication.StatusBar.SetText(" addReport_Layouttype Method Failed :  " & ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
        End Try

    End Sub

    Private Sub objApplication_AppEvent(ByVal EventType As SAPbouiCOM.BoAppEventTypes) Handles objApplication.AppEvent
        If EventType = SAPbouiCOM.BoAppEventTypes.aet_CompanyChanged Or EventType = SAPbouiCOM.BoAppEventTypes.aet_ServerTerminition Or EventType = SAPbouiCOM.BoAppEventTypes.aet_ShutDown Then
            Try
                Remove_Menu({"43520,GT"})
                DisConnect_Addon()

                ' objUIXml.LoadMenuXML("RemoveMenu.xml", Mukesh.SBOLib.UIXML.enuResourceType.Embeded)

                'If objCompany.Connected Then objCompany.Disconnect()
                'objCompany = Nothing
                'objApplication = Nothing
                'System.Runtime.InteropServices.Marshal.ReleaseComObject(objCompany)
                'System.Runtime.InteropServices.Marshal.ReleaseComObject(objApplication)
                'GC.Collect()
            Catch ex As Exception
            End Try
            End
        End If
    End Sub

    Private Sub DisConnect_Addon()
        Try
            If objAddOn.objApplication.Forms.Count > 0 Then
                Try
                    For frm As Integer = objAddOn.objApplication.Forms.Count - 1 To 0 Step -1
                        If objAddOn.objApplication.Forms.Item(frm).IsSystem = True Then Continue For
                        objAddOn.objApplication.Forms.Item(frm).Close()
                    Next
                Catch ex As Exception
                End Try

                'If objApplication.Menus.Item("43520").SubMenus.Exists(MenuID) Then objApplication.Menus.Item("43520").SubMenus.RemoveEx(MenuID)
            End If
            If objCompany.Connected Then objCompany.Disconnect()
            System.Runtime.InteropServices.Marshal.ReleaseComObject(objCompany)
            System.Runtime.InteropServices.Marshal.ReleaseComObject(objApplication)
            objCompany = Nothing
            GC.Collect()
            System.Windows.Forms.Application.Exit()
            End
        Catch ex As Exception

        End Try
    End Sub

    Private Sub Remove_Menu(ByVal MenuID() As String)
        Try
            Dim split_char() As String

            If Not MenuID Is Nothing Then
                If MenuID.Length > 0 Then
                    For i = 0 To MenuID.Length - 1
                        If Trim(MenuID(i)) = "" Then Continue For
                        split_char = MenuID(i).Split(",")
                        If split_char.Length <> 2 Then Continue For
                        If (objAddOn.objApplication.Menus.Item(split_char(0)).SubMenus.Exists(split_char(1))) Then
                            objAddOn.objApplication.Menus.Item(split_char(0)).SubMenus.RemoveEx(split_char(1))
                        End If
                    Next
                End If
            End If



        Catch ex As Exception

        End Try
    End Sub

    Private Sub objApplication_LayoutKeyEvent(ByRef eventInfo As SAPbouiCOM.LayoutKeyInfo, ByRef BubbleEvent As Boolean) Handles objApplication.LayoutKeyEvent

        ''BubbleEvent = True
        'If eventInfo.BeforeAction = True Then
        '    If eventInfo.FormUID.Contains(clsJobCard.FormType) Then
        '        objJobCard.LayoutKeyEvent(eventInfo, BubbleEvent)
        '    End If
        'End If
    End Sub

End Class




