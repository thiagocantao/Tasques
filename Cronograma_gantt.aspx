<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Cronograma_gantt.aspx.cs" Inherits="BriskPtf._Projetos.DadosProjeto._Projetos_DadosProjeto_Cronograma_gantt" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
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
            window.top.showModalComFooter("PopUpCronograma.aspx?" + tarefaParam + idProjetoParam + dataParam, traducao.Cronograma_gantt_detalhes_da_tarefa,   null,   null,           null, null      , null);           
            
        }
        function selecionaTarefa()
        {
            var idTarefa = tarefaSelecionada;
            if (tarefaSelecionada != null && idTarefa != -1)
            {
                var codigoProjeto = hfCodigoProjeto.Get("CodigoProjeto");
                
                if(idTarefa != null && idTarefa != "")
                {
                    abreDetalhes(idTarefa, codigoProjeto, "");
                }
            }
            else
            {
                window.top.mostraMensagem(traducao.Cronograma_gantt_selecione_uma_tarefa_para_visualizar_os_detalhes_, 'atencao', true, false, null);
            }
        }

        function funcaoPosModalEdicao(retorno) {
            var funcFechaModal = function () {
                window.top.pcModal.GetContentIFrameWindow().parent.existeConteudoCampoAlterado = false;
                window.top.cancelaFechamentoPopUp = 'N';
                window.top.fechaModal();
            };
            var existeInformacoesPendentes = VerificaExistenciaInformacoesPendentes();
            if (existeInformacoesPendentes) {
                var textoMsg = traducao.Cronograma_gantt_existem_altera__es_ainda_n_o_salvas_ + "</br></br>" + traducao.Cronograma_gantt_ao_pressionar__ok___as_altera__es_n_o_salvas_ser_o_perdidas_ + "</br></br>" + traducao.Cronograma_gantt_deseja_continuar_;
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

    <!--Ext and ux styles -->
    <link href="../../scripts/basicBryNTum/ext-all.css" rel="stylesheet"
        type="text/css" />

    <link href="../../Bootstrap/css/custom.css" rel="stylesheet" type="text/css" />
    <link href="../../estilos/custom.css" rel="stylesheet" type="text/css" />
    <!--Scheduler styles-->
    <link href="../../scripts/basicBryNTum/sch-gantt-all.css" rel="stylesheet" type="text/css" />
    <!--Implementation specific styles-->
    <link href="../../scripts/basicBryNTum/basic.css?v=2" rel="stylesheet" type="text/css" />
    <link href="../../scripts/basicBryNTum/examples.css" rel="stylesheet" type="text/css" />
    <!--Ext lib and UX components-->
    <script src="../../scripts/basicBryNTum/ext-all.js?v=0" type="text/javascript"></script>
    <script src="../../scripts/basicBryNTum/gnt-all-debug.js?ver=3.0.12" type="text/javascript"></script>

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

        
/*3188c0ff*/

/*.sch-gantt-parenttask-bar .sch-gantt-progress-bar {
    background: #ccdbffff;
}

.sch-gantt-task-bar, .sch-gantt-task-segment {

    border-color: #5fa2dd;
    background-color: #3188c0ff;
    color: #3188c0ff;
    padding: 1px;

}

.atrasada.sch-gantt-task-bar, .atrasada.sch-gantt-task-segment {

    border-color: #5fa2dd;
    background-color: #7342d7ff;
    color: #7342d7ff;
    padding: 1px;

}

.sch-gantt-task-bar .sch-gantt-progress-bar, .sch-gantt-task-segment .sch-gantt-progress-bar {
    background-color: #ccdbffff;
    color: #fff;
    text-align: right;
    text-indent: 3px;
    font-size: 0.9em;
    overflow: visible;
}

.sch-gantt-milestone-diamond {
    border: 0 none #b9992bff;
    background-color: #b9992bff;
    border-radius: 1px;
    border-radius: 1px;
}

.sch-gantt-parenttask-arrow {
    border-color: #4a4a4aff;
}

.sch-gantt-task-baseline .sch-gantt-task-bar {
    background: #bdbdbdff;
    border: 1px solid #0c3f5fff;
    margin-top: 2px;
}

.sch-gantt-parenttask-baseline .sch-gantt-parenttask-bar {
  display:none;
}
.sch-gantt-milestone-diamond {
    border: 0 none #b9992bff;
}*/

.sch-ganttpanel-showbaseline .sch-gantt-milestone-baseline {
    display: none;
}


.sch-dependency-line, .sch-dependency-arrow {
     border-color: #95c477; 
}

  .x-tree-node-text,  .x-grid-cell-inner {
      font-size:14px;
      }

      .adiantada .x-tree-node-text, .adiantada .x-grid-cell-inner {
        color:#0000FF;
      }

      .atrasada .x-tree-node-text, .atrasada .x-grid-cell-inner {
        color:#FF0000;
      }

      .concluido .x-tree-node-text, .concluido .x-grid-cell-inner {
        color:forestgreen;
      }

      .marco .x-tree-node-text {
        font-style:italic;
      }

      .sch-gantt-item .sch-gantt-task-bar .critica .sch-event-resizable- {
        border-radius: 1px;
        background: #7342d7 !important;
        border: 0 none;
    }
      div.critica{
        background: #7342d7;
    }
       img.critica{
        background: #7342d7;
    }
        img.concluida {
        background: forestgreen;
        
    }

    div.concluida {
        background: forestgreen;
        
    }
.x-tree-node-text {
     text-transform: none; 
}

.iconzoom-to-fit:before {
    content: "\f0b2";
    font-family: FontAwesome;
    font-style: normal;
    font-weight: normal;
    text-decoration: inherit;
    font-size: 18px;
}

.iconzoom-in:before {
    content: "\f00e";
    font-family: FontAwesome;
    font-style: normal;
    font-weight: normal;
    text-decoration: inherit;
    font-size: 18px;
}

.icon-fullscreen:before {
    content: "\f108";
    font-family: FontAwesome;
    font-style: normal;
    font-weight: normal;
    text-decoration: inherit;
    font-size: 18px;
}

.iconzoom-out:before {
    content: "\f010";
    font-family: FontAwesome;
    font-style: normal;
    font-weight: normal;
    text-decoration: inherit;
    font-size: 18px;
}
    </style>

<%--    <script type="text/javascript">
            loadingPanel.Show();
    </script>--%>


</head>
<body style="margin: 0px" onresize="App.Gantt.refresh();">
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
                                                            <dxe:ASPxImage ID="btnPDF" ClientInstanceName="btnPDF" runat="server"
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
                                                <dxcp:ASPxSpinEdit ID="txtConcluido" runat="server" ClientInstanceName="txtConcluido" MaxValue="100" NullText="%Concluído" NumberType="Integer" Width="100%" ToolTip="(%) Percentual concluído">
                                                    <SpinButtons ClientVisible="False">
                                                    </SpinButtons>
                                                </dxcp:ASPxSpinEdit>
                                            </td>
                                            <td>&nbsp;</td>
                                            <td style="width: 85px">
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
                                                <dxe:ASPxComboBox ID="ddlLinhaBase" runat="server" Width="100%" ClientInstanceName="ddlLinhaBase" TextFormatString="{0}" NullText="Linha de Base">
                                                    <ClientSideEvents SelectedIndexChanged="function(s, e) {
	var indicaAprovada = s.GetSelectedItem().GetColumnText('StatusAprovacao')  == traducao.Cronograma_gantt_aprovado;
                btnSelecionar.GetMainElement().title = indicaAprovada ? '' : traducao.Cronograma_gantt_s____poss_vel_visualizar_o_gantt_de_linhas_de_base_aprovadas;

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
                                            <td style="width: 25px">
                                                <dxe:ASPxImage ID="imgLB" runat="server" ClientInstanceName="imgLB"
                                                    Cursor="Pointer" ImageUrl="~/imagens/ajuda.png"
                                                    ToolTip="Informações da Linha de Base Selecionada">
                                                </dxe:ASPxImage>
                                            </td>
                                            <td style="width: 100px">
                                                <dxcp:ASPxButton ID="ASPxButton5" runat="server"  Text="Selecionar" Width="100%" ClientInstanceName="btnSelecionar">
                                                    <Paddings Padding="0px" />
                                                    <ClientSideEvents Click="function(s, e) {
                                                       //loadingPanel.Show();
                                                      }" />
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
                        <div id="basicGantt" style="width:100%;">        
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
                                                        <%--<td style="border: 1px solid #808080; background-color: #7342d7" width="10px">&nbsp;</td>
                                                        <td style="padding-left: 3px; padding-right: 10px">
                                                            <dxcp:ASPxLabel ID="Label3" ClientInstanceName="Label4" runat="server" Text="Críticas">
                                                            </dxcp:ASPxLabel>

                                                        </td>
                                                        <td style="border: 1px solid #808080; background-color: #bc9987;" width="10px">&nbsp;</td>
                                                        <td style="padding-left: 3px; padding-right: 10px">
                                                            <dxcp:ASPxLabel ID="Label5" ClientInstanceName="Label5" runat="server" Text="Linha de base">
                                                            </dxcp:ASPxLabel>
                                                        </td>                                                        <td style="border: 1px solid #808080; background-color: forestgreen;" width="10px">&nbsp;</td>
                                                        <%--<td style="padding-left: 3px; padding-right: 10px">
                                                            <dxcp:ASPxLabel ID="ASPxLabel8" ClientInstanceName="Label5" runat="server" Text="Concluído">
                                                            </dxcp:ASPxLabel>
                                                        </td>--%>
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
                                                 <table cellpadding="0" cellspacing="0" style="height: 15px">
                                                    <tr>
                                                        
                                                        <td style="border: 1px solid #808080; background-color: #7342d7" width="10px">&nbsp;</td>
                                                        <td style="padding-left: 3px; padding-right: 10px">
                                                            <dxcp:ASPxLabel ID="ASPxLabel11" ClientInstanceName="Label4" runat="server" Text="Críticas">
                                                            </dxcp:ASPxLabel>

                                                        </td>
                                                        <td style="border: 1px solid orange; background-color: #EEEEEE;" width="10px">&nbsp;</td>
                                                        <td style="padding-left: 3px; padding-right: 10px">
                                                            <dxcp:ASPxLabel ID="ASPxLabel12" ClientInstanceName="Label5" runat="server" Text="Linha de base">
                                                            </dxcp:ASPxLabel>
                                                        </td>
                                                        <td style="border: 1px solid #808080; background-color: forestgreen;" width="10px">&nbsp;</td>
                                                        <td style="padding-left: 3px; padding-right: 10px">
                                                            <dxcp:ASPxLabel ID="ASPxLabel13" ClientInstanceName="Label5" runat="server" Text="Concluído">
                                                            </dxcp:ASPxLabel>
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
                <tr>
                    <td align="left" style="padding-left: 5px">
                        <table cellpadding="0" cellspacing="0" class="style4">
                            <tr>
                                <td>
                                    <dxe:ASPxLabel ID="lblMsgFlash" runat="server" ClientInstanceName="lblMsgFlash"
                                        Font-Italic="True" 
                                        Text="*Para visualizar o gráfico de gantt é necessário utilizar o plugin Adobe Flash Player"
                                        Font-Bold="True" ForeColor="Red" ClientVisible="False">
                                    </dxe:ASPxLabel>
                                </td>
                                <td align="right" style="padding-right: 10px">
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

        </div>
        <dxhf:ASPxHiddenField ID="hfCodigoProjeto" runat="server" ClientInstanceName="hfCodigoProjeto">
        </dxhf:ASPxHiddenField>
        <dxhf:ASPxHiddenField ID="hfGeralTop" runat="server" ClientInstanceName="hfGeraltop">
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
                                <br />
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
</body>
</html>
