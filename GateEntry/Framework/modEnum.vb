Module modEnum
    Public definenew As Boolean = False
    Public FGValueFNC, FGValueINR As Double
    Public FGDocEntry, RDate, DEFINE As String
    Public RMValueFNC, RMValueINR, StdFNCValue, StdINRValue, ReceiptFNC, FBDINR As Double
    Public BDFNC, BDINR As Double
    Public RMDocEntry As String
    Public RMLineId, FGLineId, FGLineNO, RMLineNO, ReceiptNo, BDocEntry, BLineId As Integer
    Public ct, tt As String
    Public GForm As SAPbouiCOM.Form
    Public GE_Inward_GRPO_Draft As String
    Public BranchFlag, DefaultBranch As String
    Public bModal As Boolean = False
End Module
