<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="sobre.aspx.cs" Inherits="BriskPtf._Projetos._Projetos_sobre" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
    <script type="text/javascript" language="javascript" src="../scripts/CDIS.js"></script>
</head>
<body style="overflow:hidden">
    <form id="form1" runat="server">
    <div>
        <table cellpadding="0" cellspacing="0" style="width: 480px; height: 320px;">
            <tr>
                <td>
                </td>
                <td style="width: 8px; height: 5px">
                </td>
                <td>
                </td>
            </tr>
            <tr>
                <td align="right" style="width: 120px" valign="top">
                    <img src="../imagens/Sobre/barraLateralSobre.PNG" height="300" /></td>
                <td style="width: 8px">
                </td>
                <td align="left" valign="top">
                    <table cellpadding="0" cellspacing="0" style="width: 96%">
                        <tr>
                            <td style="height: 15px">
                                <span id="spnTitulo" runat="server" style=""></span></td>
                        </tr>
                        <tr>
                            <td style="height: 3px">
                            </td>
                        </tr>
                        <tr>
                            <td style="height: 13px">
                                <span style="">Copyright © 2009-2010 CDIS Informática
                                    LTDA.</span>
                            </td>
                        </tr>
                        <tr>
                            <td style="height: 3px">
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <span style="">
                                    <%= Resources.traducao.todos_os_direitos_reservados_%>
                                </span>
                            </td>
                        </tr>
                        <tr>
                            <td style="height: 10px">
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="lblAtualizacao" runat="server" ></asp:Label></td>
                        </tr>
                        <tr>
                            <td style="height: 157px">
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <span style="">
                                    <%= Resources.traducao.licenciado_para%>
                                    :</span></td>
                        </tr>
                        <tr>
                            <td style="border-right: royalblue 1px solid; border-top: royalblue 1px solid; border-left: royalblue 1px solid;
                                border-bottom: royalblue 1px solid; height: 62px" valign="top">
                                <span style="">
                                    <table>
                                        <tr>
                                            <td>
                                                <asp:Label ID="lblNomeCliente" runat="server" ></asp:Label></td>
                                        </tr>
                                        <tr>
                                            <td>
                                            </td>
                                        </tr>
                                    </table>
                                </span>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
        &nbsp;</div>
    </form>
</body>
</html>
