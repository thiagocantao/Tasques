<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Cronograma_gantt_flash.aspx.cs" Inherits="BriskPtf._Projetos.DadosProjeto._Projetos_DadosProjeto_Cronograma_gantt_flash" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <link href="../../estilos/custom.css" rel="stylesheet" />
    <title>Untitled Page</title>
    <script type="text/javascript" language="javascript" src="../../scripts/CDIS.js"></script>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <script type="text/javascript" src="../../scripts/AnyChart.js" language="javascript"></script>
    <script type="text/javascript" language="javascript">    
        function abreDetalhes(idTarefa, idProjeto, data)
        {
            var tarefaParam = 'CT=' + idTarefa;
            var dataParam = '&Data=' + data;
            var idProjetoParam = '&IDProjeto=' + idProjeto;
            
            window.top.showModal("PopUpCronograma.aspx?" + tarefaParam + idProjetoParam + dataParam, "Detalhes da Tarefa", 820, 500, "", null);           
            
        }
        function selecionaTarefa()
        {
            if(chartSample.getSelectedTaskInfo() != null)
            {
                var idTarefa = chartSample.getSelectedTaskInfo().id;
                var codigoProjeto = hfCodigoProjeto.Get("CodigoProjeto");
                
                if(idTarefa != null && idTarefa != "")
                {
                    abreDetalhes(idTarefa, codigoProjeto, "");
                }
            }
            else
            {
                window.top.mostraMensagem("Selecione uma Tarefa para Visualizar os Detalhes!", 'atencao', true, false, null);
            }
        }

        function funcaoPosModalEdicao(retorno) {
            var funcFechaModal = function () {
                window.top.pcModal.GetContentIFrameWindow().parent.existeConteudoCampoAlterado = false;
                window.top.cancelaFechamentoPopUp = 'N';
                window.top.fechaModal();
            }
            var existeInformacoesPendentes = VerificaExistenciaInformacoesPendentes();
            if (existeInformacoesPendentes) {
                var textoMsg = "Existem alterações ainda não salvas.</br></br>Ao pressionar 'Ok', as alterações não salvas serão perdidas!</br></br>Deseja continuar?";
                window.top.mostraMensagem(textoMsg, 'confirmacao', true, true, funcFechaModal);
                window.top.cancelaFechamentoPopUp = 'S';
            }
            else {
                cbkGeral.PerformCallback(retorno);
            }
        }

        function VerificaExistenciaInformacoesPendentes() {
            var frame = window.top.pcModal.GetContentIFrameWindow().parent;
            return frame.existeConteudoCampoAlterado;
        }

        function funcaoPosModal(retorno) {
            cbkGeral.PerformCallback(retorno);
        }

        function zoomGantt(s) {
            var myArguments = new Object();
            myArguments.param1 = s.cp_ParamXML;
            myArguments.param2 = s.cp_ParamALT;

            Thiswidth = screen.width - 80;
            var Thisheight = screen.height - 240;
            window.top.showModal('../../ZoomGantt.aspx', 'Zoom', Thiswidth, Thisheight, "", myArguments);
        }

        function verificaFlash() {
            if(navigator.plugins["Shockwave Flash"] || navigator.plugins["Shockwave Flash 2.0"])
                lblMsgFlash.SetVisible(false);
            else
                lblMsgFlash.SetVisible(true);
        }

        
        (function () {

            // Detect if ClickOnce is supported by the browser. 
            // Roughly based on http://stackoverflow.com/a/4653175/117402

            var hasMimeSupport = function () {
                var mimes = window.navigator.mimeTypes,
                    hasSupport = false;

                var mimetype = navigator.mimeTypes["application/x-ms-application"];
                if (mimetype === undefined) {
                    hasSupport = false;
                }
                else
                {
                    hasSupport = true;
                }
                

                return hasSupport;
            };

            var sniffForClickOnce = function () {
                var userAgent = window.navigator.userAgent.toUpperCase(),
                    hasNativeDotNet = userAgent.indexOf('.NET CLR 3.5') >= 0;

                return hasNativeDotNet || hasMimeSupport("application/x-ms-application");
            };

            window.clickOnce = sniffForClickOnce();

        })();

    </script>
    <style type="text/css">
        .style4 {
            width: 100%;
        }

        .style7 {
            height: 5px;
        }

        .style8 {
            width: 140px;
        }

        .style10 {
            height: 10px;
        }

        .style11 {
            width: 10px;
        }

        .style3 {
            width: 135px;
        }
    </style>
</head>
<body style="margin: 0px" onload="verificaFlash();">
    <form id="form1" runat="server">
        <div>

            <table cellpadding="0" cellspacing="0" style="width: 100%;">
                <tr>
                    <td align="left" style="height: 10px"></td>
                </tr>
                <tr>
                    <td align="left">
                        <table id="tbBotoes" runat="server" cellpadding="0" cellspacing="0"
                            style="width: 100%; background-color: #E5E5E5;">
                            <tr>
                                <td style="height: 25px">
                                    <table cellpadding="0" cellspacing="0" style="background-image: url(imagens/titulo/back_Titulo_Desktop.gif); width: 100%">
                                        <tr>
                                            <td align="center">
                                                <span id="spanBotoes" runat="server"></span>
                                            </td>
                                            <td align="left">
                                                <table>
                                                    <tr>
                                                        <td style="width: 30px" align="center">
                                                            <dxe:ASPxImage ID="imgGraficos" runat="server" ImageUrl="~/imagens/botoes/pFormulario.png"
                                                                ToolTip="Visualizar Detalhes da Tarefa Selecionada" Height="19px">
                                                                <ClientSideEvents Click="function(s, e) {
	selecionaTarefa();
}" />
                                                            </dxe:ASPxImage>
                                                        </td>
                                                        <td style="width: 30px" align="center">
                                                            <dxe:ASPxImage ID="ASPxImage1" runat="server"
                                                                ImageUrl="~/imagens/botoes/btnPDF.png" Cursor="pointer"
                                                                ToolTip="Exportar para PDF" Height="19px">
                                                                <ClientSideEvents Click="function(s, e) {
//debugger
//url += &quot;MA=&quot; + ((ckMarcos.GetChecked() == true) ? &quot;S&quot; : &quot;N&quot;);	
var url = &quot;#&quot; + ((ckMarcos.GetChecked() == true) ? &quot;S&quot; : &quot;N&quot;);	

//url += &quot;&amp;NC=&quot; + ((ckNaoConcluidas.GetChecked() == true) ? &quot;S&quot; : &quot;N&quot;);	
url += &quot;#&quot; + ((ckNaoConcluidas.GetChecked() == true) ? &quot;S&quot; : &quot;N&quot;);	

//url += &quot;&amp;TA=&quot; + ((ckTarefasAtrasadas.GetChecked() == true) ? &quot;S&quot; : &quot;N&quot;);	
url += &quot;#&quot; + ((ckTarefasAtrasadas.GetChecked() == true) ? &quot;S&quot; : &quot;N&quot;);	

//url += &quot;&amp;REC=&quot; + ddlRecurso.GetValue();	
url += &quot;#&quot; + ddlRecurso.GetValue();	

//url += &quot;&amp;CP=&quot; + hfCodigoProjeto.Get('CodigoProjeto');	
url += &quot;#&quot; + hfCodigoProjeto.Get('CodigoProjeto');	

//url += &quot;&amp;NP=&quot; + hfCodigoProjeto.Get('NomeProjeto');	
url += &quot;#&quot; + hfCodigoProjeto.Get('NomeProjeto');	

//url += &quot;&amp;NR=&quot; + ddlRecurso.GetText();
url += &quot;#&quot; + ddlRecurso.GetText();

//url += &quot;&amp;LB=&quot; + (ddlLinhaBase.GetValue() == null ? '-1' : ddlLinhaBase.GetValue());
url += &quot;#&quot; + (ddlLinhaBase.GetValue() == null ? '-1' : ddlLinhaBase.GetValue());

cbImprimeCronograma.PerformCallback(url);

}" />
                                                            </dxe:ASPxImage>
                                                        </td>
                                                        <td style="width: 30px" align="center">
                                                            <dxe:ASPxImage ID="imgAlerta" runat="server" Cursor="pointer"
                                                                ClientInstanceName="imgAlerta"
                                                                ToolTip="Alerta! Clique aqui para mais informações..." Height="19px">
                                                                <ClientSideEvents Click="function(s, e) {

popUpAlerta.Show();
}" />
                                                            </dxe:ASPxImage>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                            <td class="style3">
                                                <dxe:ASPxCheckBox ID="ckMarcos" runat="server" 
                                                    Text="Apenas os Marcos" ClientInstanceName="ckMarcos"
                                                    CheckState="Unchecked">
                                                </dxe:ASPxCheckBox>
                                            </td>
                                            <td style="width: 166px; display: none">
                                                <dxe:ASPxCheckBox ID="ckNaoConcluidas" runat="server" 
                                                    Text="Apenas Não Concluídas"
                                                    ClientInstanceName="ckNaoConcluidas" CheckState="Unchecked">
                                                </dxe:ASPxCheckBox>
                                            </td>
                                            <td class="style3">
                                                <dxe:ASPxCheckBox ID="ckTarefasAtrasadas" runat="server" 
                                                    Text="Apenas Atrasadas"
                                                    ClientInstanceName="ckTarefasAtrasadas" CheckState="Unchecked">
                                                </dxe:ASPxCheckBox>
                                            </td>
                                            <td align="right">&nbsp;</td>
                                            <td style="width: 70px">                                                		
                                                <dxcp:ASPxSpinEdit ID="txtConcluido" runat="server" ClientInstanceName="txtConcluido"  NullText="% Concluído" NumberType="Integer" Width="100%" ToolTip="(%) Concluído">
                                                    <SpinButtons ShowIncrementButtons="False">
                                                    </SpinButtons>
                                                    <NullTextStyle  ForeColor="Silver">
                                                    </NullTextStyle>
                                                </dxcp:ASPxSpinEdit>
                                            </td>
                                            <td>&nbsp;</td>
                                            <td style="width: 120px">
                                                <dxcp:ASPxDateEdit ID="ddlData" runat="server"  NullText="Data" Width="100%">
                                                    <NullTextStyle ForeColor="Silver">
                                                    </NullTextStyle>
                                                </dxcp:ASPxDateEdit>
                                            </td>
                                            <td>&nbsp;</td>
                                            <td style="width: 185px">
                                                <dxe:ASPxComboBox ID="ddlRecurso" runat="server"
                                                    Width="100%"
                                                    ClientInstanceName="ddlRecurso" IncrementalFilteringMode="Contains" NullText="Recurso">
                                                    <Paddings Padding="0px" />
                                                    <NullTextStyle ForeColor="Silver">
                                                    </NullTextStyle>
                                                </dxe:ASPxComboBox>
                                            </td>
                                            <td align="right">&nbsp;</td>
                                            <td style="width: 120px">
                                                <dxe:ASPxComboBox ID="ddlLinhaBase" runat="server"
                                                    Width="100%"
                                                    ClientInstanceName="ddlLinhaBase" TextFormatString="{0}" NullText="Linha de Base">
                                                    <ClientSideEvents SelectedIndexChanged="function(s, e) {
	var indicaAprovada = s.GetSelectedItem().GetColumnText('StatusAprovacao')  == 'Aprovada';
                btnSelecionar.GetMainElement().title = indicaAprovada ? '' : 'Só é possível visualizar o gantt de linhas de base aprovadas';

	btnSelecionar.SetEnabled(indicaAprovada);
		pcLB.PerformCallback();
}" />
                                                    <Columns>
                                                        <dxe:ListBoxColumn Caption="Versão" FieldName="VersaoLinhaBase" Width="100px" />
                                                        <dxe:ListBoxColumn Caption="Status" FieldName="StatusAprovacao" Width="150px" />
                                                    </Columns>
                                                    <Paddings Padding="0px" />
                                                    <NullTextStyle ForeColor="Silver">
                                                    </NullTextStyle>
                                                </dxe:ASPxComboBox>
                                            </td>
                                            <td style="padding-left: 3px; width: 25px">
                                                <dxe:ASPxImage ID="imgLB" runat="server" ClientInstanceName="imgLB"
                                                    Cursor="Pointer" ImageUrl="~/imagens/ajuda.png"
                                                    ToolTip="Informações da Linha de Base Selecionada">
                                                </dxe:ASPxImage>
                                            </td>
                                            <td style="width: 100px">
                                                <dxcp:ASPxButton ID="ASPxButton5" runat="server"  Text="Selecionar" Width="100%" ClientInstanceName="btnSelecionar">
                                                    <Paddings Padding="0px" />
                                                </dxcp:ASPxButton>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
                <tr>
                    <td align="left">
                        <div id="divChart">
                        </div>
                        <%=nenhumGrafico %>
                    </td>
                </tr>
                <tr>
                    <td align="left">
                        <table id="tbLegenda" runat="server" cellpadding="0" cellspacing="0"
                            style="width: 100%; background-color: #E5E5E5; height: 25px;">
                            <tr>
                                <td>
                                    <table cellpadding="0" cellspacing="0" class="style4">
                                        <tr>
                                            <td style="padding-left: 10px">
                                                <table cellpadding="0" cellspacing="0" style="height: 15px">
                                                    <tr>
                                                        <td style="padding-left: 3px; padding-right: 10px">
                                                            <span>
                                                                <dxcp:ASPxLabel ID="lblNum" ClientInstanceName="lblNum" runat="server" Text="Atrasadas" ForeColor="Red" >
                                                            </dxcp:ASPxLabel>
                                                            </span>
                                                            
                                                        </td>
                                                        <td style="padding-left: 3px; padding-right: 10px">
                                                            <dxcp:ASPxLabel ID="Label4" ClientInstanceName="Label4" runat="server" Text="Adiantadas" ForeColor="Blue" >
                                                            </dxcp:ASPxLabel>

                                                        </td>
                                                        <td style="border: 1px solid #808080; background-color: #7342d7" width="10px">&nbsp;</td>
                                                        <td style="padding-left: 3px; padding-right: 10px">
                                                            <dxcp:ASPxLabel ID="Label3" ClientInstanceName="Label4" runat="server" Text="Críticas">
                                                            </dxcp:ASPxLabel>

                                                        </td>
                                                        <td style="border: 1px solid #808080; background-color: #BFBFBF;" width="10px">&nbsp;</td>
                                                        <td style="padding-left: 3px; padding-right: 10px">
                                                            <dxcp:ASPxLabel ID="Label5" ClientInstanceName="Label5" runat="server" Text="Linha de base">
                                                            </dxcp:ASPxLabel>
                                                        </td>
                                                        <td style="padding-left: 3px; padding-right: 10px">
                                                            <span>
                                                              <dxcp:ASPxLabel ID="Label1" ClientInstanceName="Label1" runat="server" Font-Italic="True" Text="Marcos em itálico">
                                                            </dxcp:ASPxLabel>
                                                            </span>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                            <td align="right">
                                                <table>
                                                    <tr>
                                                        <td align="left" style="padding-right: 10px">&nbsp;</td>
                                                        <td style="padding-left: 5px">
                                                            <dxe:ASPxButton runat="server" AutoPostBack="False"
                                                                ClientInstanceName="btnDesbloquear" Text="Desbloquear" Width="118px"
                                                                 ID="btnDesbloquear" Height="25px">
                                                                <ClientSideEvents Click="function(s, e) {
	pcDesbloqueio.Show();
	e.processOnServer = false;
}"></ClientSideEvents>

                                                                <Paddings Padding="0px"></Paddings>
                                                            </dxe:ASPxButton>

                                                        </td>
                                                        <td style="padding-left: 5px">
                                                            <dxe:ASPxButton runat="server" Text="Editar Cronograma" Width="128px"
                                                                 ID="btnEditarCronograma"
                                                                OnClick="btnEditarCronograma_Click" Wrap="False" Height="25px">
                                                                <Paddings Padding="0px"></Paddings>
                                                            </dxe:ASPxButton>

                                                        </td>
                                                        <td style="padding-left: 5px">
                                                            <dxe:ASPxButton runat="server" AutoPostBack="False"
                                                                ClientInstanceName="btnVisualizarEAP" Text="Visualizar EAP" Width="118px"
                                                                 ID="btnVisualizarEAP" Height="25px">
                                                                <Paddings Padding="0px"></Paddings>
                                                            </dxe:ASPxButton>

                                                        </td>
                                                        <td style="padding-left: 5px; padding-right: 3px">
                                                            <dxe:ASPxButton runat="server" AutoPostBack="False"
                                                                ClientInstanceName="btnEditarEAP" Text="Editar EAP" Width="118px"
                                                                 ID="btnEditarEAP" Height="25px">
                                                                <Paddings Padding="0px"></Paddings>
                                                            </dxe:ASPxButton>

                                                        </td>
                                                        <td>
                                                            <dxe:ASPxLabel ID="lblMsgFlash" runat="server" ClientInstanceName="lblMsgFlash"
                                                                Font-Italic="True" 
                                                                Text="*Para visualizar o gráfico de gantt é necessário utilizar o plugin Adobe Flash Player"
                                                                Font-Bold="True" ForeColor="Red" ClientVisible="False">
                                                            </dxe:ASPxLabel>
                                                        </td>
                                                        <td align="right" style="padding-right: 7px">
                                                            <dxe:ASPxImage ID="imgClickOnce" runat="server" ClientInstanceName="imgClickOnce"
                                                                Cursor="Pointer" ImageUrl="~/imagens/ajuda.png"
                                                                ToolTip="Ajuda para abrir o cronograma" ClientVisible="False">
                                                            </dxe:ASPxImage>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>

        </div>
        <dxhf:ASPxHiddenField ID="hfCodigoProjeto" runat="server" ClientInstanceName="hfCodigoProjeto">
        </dxhf:ASPxHiddenField>
        <dxpc:ASPxPopupControl ID="pcLB" runat="server" ClientInstanceName="pcLB"
            
            HeaderText="Informações da Linha de Base Selecionada" PopupElementID="imgLB"
            Width="500px">
            <SettingsLoadingPanel Enabled="False" />
            <ContentCollection>
                <dxpc:PopupControlContentControl runat="server" SupportsDisabledAttribute="True">
                    <table cellspacing="1" width="100%">
                        <tr>
                            <td>
                                <table cellspacing="1" class="style4">
                                    <tr>
                                        <td class="style8">
                                            <dxe:ASPxLabel ID="ASPxLabel1" runat="server"
                                                Text="Versão:">
                                            </dxe:ASPxLabel>
                                        </td>
                                        <td>
                                            <dxe:ASPxLabel ID="ASPxLabel2" runat="server"
                                                Text="Status:">
                                            </dxe:ASPxLabel>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="style8" style="padding-right: 10px">
                                            <dxe:ASPxTextBox ID="txtVersao" runat="server" ClientEnabled="False"
                                                 Width="100%">
                                                <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                </DisabledStyle>
                                            </dxe:ASPxTextBox>
                                        </td>
                                        <td>
                                            <dxe:ASPxTextBox ID="txtStatus" runat="server" ClientEnabled="False"
                                                 Width="100%">
                                                <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                </DisabledStyle>
                                            </dxe:ASPxTextBox>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td class="style7"></td>
                        </tr>
                        <tr>
                            <td>
                                <table cellspacing="1" class="style4">
                                    <tr>
                                        <td class="style8">
                                            <dxe:ASPxLabel ID="ASPxLabel3" runat="server"
                                                Text="Data Solicitação:">
                                            </dxe:ASPxLabel>
                                        </td>
                                        <td>
                                            <dxe:ASPxLabel ID="ASPxLabel4" runat="server"
                                                Text="Solicitante:">
                                            </dxe:ASPxLabel>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="style8" style="padding-right: 10px">
                                            <dxe:ASPxTextBox ID="txtDataSolicitacao" runat="server" ClientEnabled="False"
                                                 Width="100%"
                                                DisplayFormatString="{0:dd/MM/yyyy HH:mm:ss}">
                                                <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                </DisabledStyle>
                                            </dxe:ASPxTextBox>
                                        </td>
                                        <td>
                                            <dxe:ASPxTextBox ID="txtSolicitante" runat="server" ClientEnabled="False"
                                                 Width="100%">
                                                <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                </DisabledStyle>
                                            </dxe:ASPxTextBox>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td class="style7"></td>
                        </tr>
                        <tr>
                            <td>
                                <table cellspacing="1" class="style4">
                                    <tr>
                                        <td class="style8">
                                            <dxe:ASPxLabel ID="ASPxLabel5" runat="server"
                                                Text="Data Aprov./Reprov.:">
                                            </dxe:ASPxLabel>
                                        </td>
                                        <td>
                                            <dxe:ASPxLabel ID="ASPxLabel6" runat="server"
                                                Text="Aprov./Reprov. por:">
                                            </dxe:ASPxLabel>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="style8" style="padding-right: 10px">
                                            <dxe:ASPxTextBox ID="txtDataAprovacao" runat="server" ClientEnabled="False"
                                                 Width="100%"
                                                DisplayFormatString="{0:dd/MM/yyyy HH:mm:ss}">
                                                <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                </DisabledStyle>
                                            </dxe:ASPxTextBox>
                                        </td>
                                        <td>
                                            <dxe:ASPxTextBox ID="txtAprovador" runat="server" ClientEnabled="False"
                                                 Width="100%">
                                                <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                </DisabledStyle>
                                            </dxe:ASPxTextBox>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td class="style7"></td>
                        </tr>
                        <tr>
                            <td>
                                <dxe:ASPxLabel ID="ASPxLabel7" runat="server"
                                    Text="Descrição da Solicitação:">
                                </dxe:ASPxLabel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <dxe:ASPxMemo ID="txtAnotacao" runat="server" ClientEnabled="False"
                                     Rows="8" Width="100%">
                                    <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                    </DisabledStyle>
                                </dxe:ASPxMemo>
                            </td>
                        </tr>
                    </table>
                </dxpc:PopupControlContentControl>
            </ContentCollection>
        </dxpc:ASPxPopupControl>
        <dxpc:ASPxPopupControl ID="pcInformacao" runat="server" ClientInstanceName="pcInformacao"
            
            HeaderText="Informação"
            Width="500px" Modal="True" PopupHorizontalAlign="WindowCenter"
            PopupVerticalAlign="WindowCenter" CloseAction="CloseButton">
            <ContentCollection>
                <dxpc:PopupControlContentControl runat="server" SupportsDisabledAttribute="True">
                    <table cellspacing="1" class="style4">
                        <tr>
                            <td>
                                <dxe:ASPxLabel ID="lblInformacao" runat="server"
                                    ClientInstanceName="lblInformacao" >
                                </dxe:ASPxLabel>
                            </td>
                        </tr>
                        <tr>
                            <td class="style10"></td>
                        </tr>
                        <tr>
                            <td>
                                <table cellspacing="1" class="style4">
                                    <tr>
                                        <td align="right">
                                            <dxe:ASPxButton ID="btnAbrirCronoBloqueado" runat="server"
                                                Text="Sim" Width="80px" AutoPostBack="False">
                                                <Paddings Padding="0px" />
                                            </dxe:ASPxButton>
                                        </td>
                                        <td class="style11">&nbsp;</td>
                                        <td>
                                            <dxe:ASPxButton ID="ASPxButton3" runat="server"
                                                Text="Não" Width="80px" AutoPostBack="False">
                                                <ClientSideEvents Click="function(s, e) {
	pcInformacao.Hide();
}" />
                                                <Paddings Padding="0px" />
                                            </dxe:ASPxButton>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </dxpc:PopupControlContentControl>
            </ContentCollection>
        </dxpc:ASPxPopupControl>
        <dxpc:ASPxPopupControl ID="pcInformacaoEAP" runat="server" ClientInstanceName="pcInformacaoEAP"
            
            HeaderText="Informação"
            Width="500px" Modal="True" PopupHorizontalAlign="WindowCenter"
            PopupVerticalAlign="WindowCenter" CloseAction="CloseButton">
            <ContentCollection>
                <dxpc:PopupControlContentControl runat="server" SupportsDisabledAttribute="True">
                    <table cellspacing="1" class="style4">
                        <tr>
                            <td>
                                <dxe:ASPxLabel ID="lblInformacaoEAP" runat="server"
                                    ClientInstanceName="lblInformacaoEAP" >
                                </dxe:ASPxLabel>
                            </td>
                        </tr>
                        <tr>
                            <td class="style10"></td>
                        </tr>
                        <tr>
                            <td>
                                <table cellspacing="1" class="style4">
                                    <tr>
                                        <td align="right">
                                            <dxe:ASPxButton ID="btnAbrirCronoBloqueadoEAP" runat="server"
                                                Text="Sim" Width="80px" AutoPostBack="False">
                                                <Paddings Padding="0px" />
                                            </dxe:ASPxButton>
                                        </td>
                                        <td class="style11">&nbsp;</td>
                                        <td>
                                            <dxe:ASPxButton ID="ASPxButton6" runat="server"
                                                Text="Não" Width="80px" AutoPostBack="False">
                                                <ClientSideEvents Click="function(s, e) {
	pcInformacaoEAP.Hide();
}" />
                                                <Paddings Padding="0px" />
                                            </dxe:ASPxButton>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </dxpc:PopupControlContentControl>
            </ContentCollection>
        </dxpc:ASPxPopupControl>
        <dxpc:ASPxPopupControl ID="pcDownload" runat="server" ClientInstanceName="pcDownload"
            
            HeaderText="Informação"
            Width="500px" Modal="True" PopupHorizontalAlign="WindowCenter"
            PopupVerticalAlign="WindowCenter" Height="112px">
            <ClientSideEvents Shown="function(s, e) {
	setTimeout(&quot;pcDownload.Hide()&quot;,10000);
}" />
            <ContentCollection>
                <dxpc:PopupControlContentControl runat="server" SupportsDisabledAttribute="True">
                    <dxe:ASPxLabel ID="lblDownload" runat="server" ClientInstanceName="lblDownload"
                        EncodeHtml="False" 
                        Text="Download &lt;a href='#'&gt;Aqui...&lt;/a&gt;">
                    </dxe:ASPxLabel>
                </dxpc:PopupControlContentControl>
            </ContentCollection>
        </dxpc:ASPxPopupControl>
        <dxpc:ASPxPopupControl ID="pcDownloadOnce" runat="server" ClientInstanceName="pcDownloadOnce"
            
            HeaderText="Informação"
            Width="500px" Modal="True" MinHeight="100px" PopupElementID="imgClickOnce">
            <ClientSideEvents Shown="function(s, e) {
	setTimeout(&quot;pcDownload.Hide()&quot;,10000);
}" />
            <ContentCollection>
                <dxpc:PopupControlContentControl runat="server" SupportsDisabledAttribute="True">
                    <dxe:ASPxLabel ID="lblDownloadOnce" runat="server" ClientInstanceName="lblDownloadOnce"
                        EncodeHtml="False" 
                        Text="Download &lt;a href='#'&gt;Aqui...&lt;/a&gt;">
                    </dxe:ASPxLabel>
                </dxpc:PopupControlContentControl>
            </ContentCollection>
        </dxpc:ASPxPopupControl>
        <dxcb:ASPxCallback ID="cbkGeral" runat="server" ClientInstanceName="cbkGeral" OnCallback="cbkGeral_Callback">
            <ClientSideEvents CallbackComplete="function(s, e) {
	location.reload();
}" />
        </dxcb:ASPxCallback>
        <dxpc:ASPxPopupControl ID="pcDesbloqueio" runat="server" ClientInstanceName="pcDesbloqueio"
            
            HeaderText="Informação"
            Width="500px" Modal="True" PopupHorizontalAlign="WindowCenter"
            PopupVerticalAlign="WindowCenter" CloseAction="CloseButton">
            <ContentCollection>
                <dxpc:PopupControlContentControl runat="server" SupportsDisabledAttribute="True">
                    <table cellspacing="1" class="style4">
                        <tr>
                            <td>
                                <dxe:ASPxLabel ID="lblDesbloqueio" runat="server"
                                    ClientInstanceName="lblDesbloqueio" >
                                </dxe:ASPxLabel>
                            </td>
                        </tr>
                        <tr>
                            <td class="style10"></td>
                        </tr>
                        <tr>
                            <td>
                                <table cellspacing="1" class="style4">
                                    <tr>
                                        <td align="right">
                                            <dxe:ASPxButton ID="btnDesbloquearCrono" runat="server"
                                                Text="Sim" Width="80px"
                                                OnClick="btnDesbloquearCrono_Click">
                                                <ClientSideEvents Click="function(s, e) {
	pcDesbloqueio.Hide();
}" />
                                                <Paddings Padding="0px" />
                                            </dxe:ASPxButton>
                                        </td>
                                        <td class="style11">&nbsp;</td>
                                        <td>
                                            <dxe:ASPxButton ID="ASPxButton4" runat="server"
                                                Text="Não" Width="80px" AutoPostBack="False">
                                                <ClientSideEvents Click="function(s, e) {
	pcDesbloqueio.Hide();
}" />
                                                <Paddings Padding="0px" />
                                            </dxe:ASPxButton>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </dxpc:PopupControlContentControl>
            </ContentCollection>
        </dxpc:ASPxPopupControl>
        <dxpc:ASPxPopupControl ID="popupAlerta" runat="server" ClientInstanceName="popUpAlerta"
            Width="458px" 
            HeaderText="Atenção" PopupElementID="imgAlerta" PopupHorizontalOffset="10"
            PopupVerticalOffset="-60">
            <ContentStyle HorizontalAlign="Left">
            </ContentStyle>
            <ContentCollection>
                <dxpc:PopupControlContentControl ID="PopupControlContentControl1" runat="server">
                    <table cellpadding="0" cellspacing="0" class="style4">
                        <tr>
                            <td>
                                <dxe:ASPxLabel ID="lblAlerta" runat="server" EncodeHtml="False"
                                    >
                                </dxe:ASPxLabel>

                            </td>
                        </tr>
                        <tr>
                            <td class="style10"></td>
                        </tr>
                        <tr>
                            <td align="right">
                                <dxe:ASPxButton ID="btnEditarCronograma2" runat="server"
                                    OnClick="btnEditarCronograma_Click" Text="Editar Cronograma"
                                    Width="130px">
                                    <Paddings Padding="0px" />
                                </dxe:ASPxButton>
                            </td>
                        </tr>
                    </table>
                </dxpc:PopupControlContentControl>
            </ContentCollection>
        </dxpc:ASPxPopupControl>
        <dxcb:ASPxCallback ID="cbImprimeCronograma" runat="server"
            ClientInstanceName="cbImprimeCronograma"
            OnCallback="cbImprimeCronograma_Callback">
            <ClientSideEvents CallbackComplete="function(s, e)
{
	if(s.cp_MensagemErro == &quot;OK&quot;)
    {
		window.location = '../../_Processos/Visualizacao/ExportacaoDados.aspx?exportType=pdf&amp;amp;bInline=False';
    }
	else
	{
		window.top.mostraMensagem(s.cp_MensagemErro, 'erro', true, false, null);
	}		
}" />
        </dxcb:ASPxCallback>
    </form>
    <script type="text/javascript" language="JavaScript">
        var chartSample = new AnyChart('./../../flashs/AnyGantt.swf');
        chartSample.width = '100%';
        chartSample.height = <%=alturaGrafico %>;
        chartSample.setXMLFile('<%=grafico_xml %>');
        chartSample.write('divChart');
    </script>
</body>
</html>
