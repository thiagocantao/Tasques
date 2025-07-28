<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Cronograma_gantt1.aspx.cs"
    Inherits="BriskPtf._Projetos.DadosProjeto._Projetos_DadosProjeto_Cronograma_gantt1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <style>
        .iconAnterior
        {
            background-image: url('../../imagens/gantt/anterior.png')
        }
        .iconProximo
        {
            background-image: url('../../imagens/gantt/proximo.png')
        }
        .iconIdentar
        {
            background-image: url('../../imagens/gantt/identar.gif')
        }
        .iconDesidentar
        {
            background-image: url('../../imagens/gantt/desidentar.gif')
        }
        .iconFechar
        {
            background-image: url('../../imagens/gantt/fechar.png')
        }
        .iconExpandir
        {
            background-image: url('../../imagens/gantt/expandir.png')
        }
        .iconIncluir
        {
            background-image: url('../../imagens/gantt/incluir.png')
        }
        .iconEditar
        {
            background-image: url('../../imagens/gantt/editar.png')
        }
        .iconExcluir
        {
            background-image: url('../../imagens/gantt/excluir.png')
        }
        .iconSalvar
        {
            background-image: url('../../imagens/gantt/salvar.png')
        }

        .auto-style1 {
            width: 125px;
        }
        .auto-style2 {
            width: 127px;
        }

    </style>
<meta http-equiv="content-type" content="text/html; charset=UTF-8">

    <title>Untitled Page</title>

    <!--Ext and ux styles -->
    <link href="../../scripts/basicBryNTum/ext-all.css" rel="stylesheet"
        type="text/css" />
        
   
    <!--Scheduler styles-->
    <link href="../../scripts/basicBryNTum/sch-gantt-all.css" rel="stylesheet" type="text/css" />
    <!--Implementation specific styles-->
    <link href="../../scripts/basicBryNTum/basic.css" rel="stylesheet" type="text/css" />
    <link href="../../scripts/basicBryNTum/examples.css" rel="stylesheet" type="text/css" />
    <!--Ext lib and UX components-->
    <script src="../../scripts/basicBryNTum/ext-all.js" type="text/javascript"></script>
    <script src="../../scripts/basicBryNTum/gnt-all-debug.js?v=2" type="text/javascript"></script>
   
</head>
<body style="margin: 0px">
    <form id="form1" runat="server">
    <div style="width:100%">
        <div id="basicGantt" style="width:100%;">
        </div>
    </div>
    <dxhf:ASPxHiddenField ID="hfCodigoProjeto" runat="server" ClientInstanceName="hfCodigoProjeto">
    </dxhf:ASPxHiddenField>
    
        <dxcp:ASPxCallback ID="callbackSalvar" runat="server" ClientInstanceName="callbackSalvar" OnCallback="callbackSalvar_Callback">
        </dxcp:ASPxCallback>
    
        <dxcp:ASPxPopupControl ID="pcEdicao" runat="server" ClientInstanceName="pcEdicao"  HeaderText="Edição de Tarefa" Modal="True" PopupHorizontalAlign="WindowCenter" PopupVerticalAlign="WindowCenter" Width="700px">
            <ContentCollection>
<dxcp:PopupControlContentControl runat="server">
    <table cellspacing="0" class="sch-fieldcontainer-label-wrap">
        <tr>
            <td>
                <dxtv:ASPxLabel ID="ASPxLabel1" runat="server"  Text="Tarefa:">
                </dxtv:ASPxLabel>
            </td>
        </tr>
        <tr>
            <td style="padding-bottom: 10px">
                <dxtv:ASPxTextBox ID="txtTarefa" runat="server" ClientInstanceName="txtTarefa"  Width="100%">
                </dxtv:ASPxTextBox>
            </td>
        </tr>
        <tr>
            <td style="padding-bottom: 10px">
                <table cellspacing="0" class="sch-fieldcontainer-label-wrap">
                    <tr>
                        <td class="auto-style1" style="padding-right: 10px">
                            <dxtv:ASPxLabel ID="ASPxLabel2" runat="server"  Text="Início:">
                            </dxtv:ASPxLabel>
                        </td>
                        <td class="auto-style1" style="padding-right: 10px">
                            <dxtv:ASPxLabel ID="ASPxLabel3" runat="server"  Text="Término:">
                            </dxtv:ASPxLabel>
                        </td>
                        <td>
                            <dxtv:ASPxLabel ID="ASPxLabel4" runat="server"  Text="%Concluído:">
                            </dxtv:ASPxLabel>
                        </td>
                    </tr>
                    <tr>
                        <td class="auto-style1" style="padding-right: 10px">
                            <dxtv:ASPxDateEdit ID="ddlInicio" runat="server" ClientInstanceName="ddlInicio"  Width="100%">
                            </dxtv:ASPxDateEdit>
                        </td>
                        <td class="auto-style1" style="padding-right: 10px">
                            <dxtv:ASPxDateEdit ID="ddlTermino" runat="server" ClientInstanceName="ddlTermino"  Width="100%">
                            </dxtv:ASPxDateEdit>
                        </td>
                        <td>
                            <dxtv:ASPxSpinEdit ID="txtPercentual" runat="server" ClientInstanceName="txtPercentual"  NumberType="Integer" Width="90px">
                            </dxtv:ASPxSpinEdit>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td align="right">
                <dxtv:ASPxButton ID="ASPxButton1" runat="server" AutoPostBack="False"  Text="OK" Width="100px">
                    <ClientSideEvents Click="function(s, e) {
	salvaTarefa();
}" />
                </dxtv:ASPxButton>
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
        </tr>
    </table>
                </dxcp:PopupControlContentControl>
</ContentCollection>
        </dxcp:ASPxPopupControl>
    
    </form>
    
</body>
</html>
