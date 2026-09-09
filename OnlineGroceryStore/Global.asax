<%@ Application Language="VB" %>
<script runat="server">
    Sub Application_Error(sender As Object, e As EventArgs)
        Dim ex = Server.GetLastError()
        System.Diagnostics.Trace.TraceError(ex.ToString())
    End Sub
</script>
