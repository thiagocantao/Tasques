<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="zoom.aspx.cs" Inherits="BriskPtf._Projetos._Projetos_zoom" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <base target="_self"/>
    <link href="../estilos/cdisEstilos.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="../scripts/FusionCharts.js?v=1" language="javascript"></script>
    <script type="text/javascript" src="../scripts/FusionChartsExportComponent.js" language="javascript"></script>
    
    
    <script type="text/javascript" language="javascript">
    function redimensiona(altura){
        document.getElementById('tbExterna').style.height=altura;
    }    
    </script>

</head>
<body style="margin: 0px;">
    <form id="form1" runat="server">
        <div id="Externo" style="width: 100%; height: 100%; z-index: 3;">
            <table style="width: 100%; height: 100%;" border="0" cellpadding="0" cellspacing="0"
                id="tbExterna">
                <tr>
                    <td style="width: 100%;" valign="middle" id="menu">
                        <table border="0" cellpadding="0" cellspacing="0" 
                            style="width: 100%; display: none;">
                            <tr>
                                <td style="height: 22px;" align="right">
                                    &nbsp;</td>
                                <td style="width: 50px; height: 22px;">
                                    &nbsp;<asp:Label ID="lblSair" Style="cursor: pointer;" runat="server" Text=" Sair" onClick="javascript:window.top.fechaModal();" ></asp:Label></td>
                            </tr>
                        </table>
                                    <table>
                                        <tr>
                                            <td align="center">
                                                <asp:Label ID="lbl_TituloGrafico" runat="server" Font-Bold="False"
                                                    Font-Size="11pt"></asp:Label></td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <div id="chartdiv" style="z-index: 2;" align="center">
                                                </div>
                                                <script type="text/javascript">
                                                    getGrafico('<%=grafico_swf %>', "grafico_1", '890', '600', '<%=grafico_xml %>', 'chartdiv');
                                                </script>
                                                 

                                            </td>
                                        </tr>
                                    </table>
                    </td>
                </tr>
            </table>
        </div>
    </form>

  
</body>
</html>