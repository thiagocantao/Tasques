<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="zoomBullets.aspx.cs" Inherits="BriskPtf._Projetos._Projetos_zoomBullets" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <link href="../estilos/cdisEstilos.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript" src="../scripts/FusionCharts.js?v=1" language="javascript"></script>
    

    <script type="text/javascript" language="javascript">
       
    function redimensiona(altura){
        document.getElementById('tbExterna').style.height=altura;
    }
    </script>

</head>
<body style="margin: 0px;">
    <form id="form1" runat="server">
        <div id="Externo" style="width: 100%; height: 100%;">
            <table style="width: 100%; height: 100%;" border="0" cellpadding="0" cellspacing="0"
                id="tbExterna">
                <tr>
                    <td style="width: 100%;" valign="middle" id="menu">
                        <table border="0" cellpadding="0" cellspacing="0" 
                            style="width: 100%; display: none;">
                            <tr>
                                <td style="height: 12px; width: 800px;" align="right">
                                </td>                                
                                <td style="width: 50px; height: 22px;">
                                    &nbsp;<asp:Label ID="Label4" Style="cursor: pointer;" runat="server" Text=" Sair" onClick="javascript:window.top.fechaModal();"></asp:Label></td>
                            </tr>
                        </table>
                                    <table border="0" cellpadding="0" cellspacing="3" style="width: 99%" id="tbGraficoPrint">
                                <tr>
                                    <td align="center">
                                    <asp:Label ID="lbl_TituloGrafico" runat="server" Font-Bold="False" Font-Size="11pt"></asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                   <td>                                    
                                     <!-- Gráfico -->
                                     <div id="vchartdiv1" align="center"></div>
                                     <script type="text/javascript">
                                      getGrafico('<%=grafico1_swf %>', "grafico001", '880', '<%=alturaGrafico %>', '<%=grafico1_xml %>', 'vchartdiv1');
                                     </script>                                  
                                   </td>
                                </tr>
                                <tr>
                                    <td style="height: 40px">
                                    </td>
                                </tr>                                
                                <tr>
                                   <td>                                    
                                     <!-- Gráfico -->
                                     <div id="vchartdiv2" align="center"></div>
                                     <script type="text/javascript">
                                        getGrafico('<%=grafico2_swf %>', "grafico002", '880', '<%=alturaGrafico %>', '<%=grafico2_xml %>', 'vchartdiv2');
                                     </script>                                      
                                  </td>
                                </tr>
                                <tr>
                                    <td style="height: 40px">
                                    </td>
                                </tr>
                                <tr>
                                   <td>                                    
                                     <!-- Gráfico -->
                                     <div id="vchartdiv3" align="center"></div>
                                     <script type="text/javascript">
                                     getGrafico('<%=grafico3_swf %>', "grafico003", '880', '<%=alturaGrafico %>', '<%=grafico3_xml %>', 'vchartdiv3');
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
<script type="text/javascript" language="javascript">
    var oMyObject = window.dialogArguments;
    var tituloGrafico = oMyObject.tituloGrafico;
    document.getElementById('lbl_TituloGrafico').innerText = tituloGrafico;
</script>
