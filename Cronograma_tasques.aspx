<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Cronograma_tasques.aspx.cs" Inherits="BriskPtf._Projetos.DadosProjeto._Projetos_DadosProjeto_Cronograma_tasques" %>

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
            
            window.top.showModal("PopUpCronograma.aspx?" + tarefaParam + idProjetoParam + dataParam, "Detalhes da Tarefa", 820, 400, "", null);           
            
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

    
    <!--Scheduler styles-->
    <link href="../../scripts/basicBryNTum/sch-gantt-all.css" rel="stylesheet" type="text/css" />
    <!--Implementation specific styles-->
    <link href="../../scripts/basicBryNTum/basic.css" rel="stylesheet" type="text/css" />
    <link href="../../scripts/basicBryNTum/examples.css" rel="stylesheet" type="text/css" />
    <!--Ext lib and UX components-->
    <script src="../../scripts/basicBryNTum/ext-all.js" type="text/javascript"></script>
    <script src="../../scripts/basicBryNTum/gnt-all-debug.js?v=2" type="text/javascript"></script>

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

      .marco .x-tree-node-text {
        font-style:italic;
      }

.x-tree-node-text {
     text-transform: none; 
}
    </style>

<%--    <script type="text/javascript">
            loadingPanel.Show();
    </script>--%>


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
                                    &nbsp;</td>
                            </tr>
                        </table>
                    </td>
                </tr>
                <tr>
                    <td align="left">
<%--                        <div id="divChart">
                        </div>--%>
                     <div style="width:100%">
                        <div id="basicGantt" style="width:100%;">
                    </div>
                    </div>

                        <%--<%=nenhumGrafico %>--%>
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
                                                            &nbsp;</td>
                                                        <td style="padding-left: 5px">
                                                            &nbsp;</td>
                                                        <td style="padding-left: 5px">
                                                            &nbsp;</td>
                                                        <td style="padding-left: 5px; padding-right: 3px">
                                                            &nbsp;</td>
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
    </form>
</body>
</html>
