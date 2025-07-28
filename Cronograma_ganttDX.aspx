<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Cronograma_ganttDX.aspx.cs" Inherits="BriskPtf._Projetos.DadosProjeto._Projetos_DadosProjeto_Cronograma_ganttDX" %>

<%@ Register Assembly="DevExpress.Web.ASPxGantt.v23.2, Version=23.2.8.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.ASPxGantt" TagPrefix="dx" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Untitled Page</title>
    <script type="text/javascript" language="javascript" src="../../scripts/CDIS.js"></script>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <script type="text/javascript" src="../../scripts/AnyChart.js" language="javascript"></script>
     <script type="text/javascript" language="javascript">
         function editaTarefa(s, e)
         {
             s.UpdateTask(s.GetFocusedTaskKey(), { Duracao: "5" });
         }

        function funcaoPosModalEdicao(retorno) {
            var funcFechaModal = function () {
                window.top.pcModal.GetContentIFrameWindow().parent.existeConteudoCampoAlterado = false;
                window.top.cancelaFechamentoPopUp = 'N';
                window.top.fechaModal();
            };
            var existeInformacoesPendentes = VerificaExistenciaInformacoesPendentes();
            if (existeInformacoesPendentes) {
                var textoMsg = traducao.Cronograma_gantt_existem_alteracoes_ainda_nao_salvas_ + "</br></br>" +
                    traducao.Cronograma_gantt_ao_pressionar__ok___as_alteracoes_nao_salvas_serao_perdidas_ + "</br></br>" +
                    traducao.Cronograma_gantt_deseja_continuar_;
                window.top.mostraMensagem(textoMsg, 'confirmacao', true, true, funcFechaModal);
                window.top.cancelaFechamentoPopUp = 'S';
            }
            else {
                location.reload();
            }
        }

        function VerificaExistenciaInformacoesPendentes() {
            var frame = window.top.pcModal.GetContentIFrameWindow().parent;
            return frame.existeConteudoCampoAlterado;
        }

        function funcaoPosModal(retorno) {
            location.reload();
        }
     </script>
    <style type="text/css">


        .style3 {
            width: 135px;
        }

        
        .style4 {
            width: 100%;
        }

        .style8 {
            width: 140px;
        }

        .style7 {
            height: 5px;
        }

        .style10 {
            height: 10px;
        }

        .style11 {
            width: 10px;
        }

        .auto-style1 {
            width: 257px;
        }

        .auto-style2 {
            width: 95px;
        }

        </style>

</head>
<body style="margin: 0px" >
    <form id="form1" runat="server">
        <div>
            <table cellpadding="0" cellspacing="0" width="100%" class="dx-justification">
                <tr>
                    <td>
                                    <table cellpadding="0" cellspacing="0" style="background-image: url(imagens/titulo/back_Titulo_Desktop.gif); width: 100%">
                                        <tr>
                                            <td align="left">
                                                <table>
                                                    <tr>
                                                        <td style="width: 30px" align="center">
                                                            <dxe:ASPxImage ID="btnPDF" ClientInstanceName="btnPDF" runat="server"
                                                                ImageUrl="~/imagens/botoes/btnPDF.png" Cursor="pointer"
                                                                ToolTip="Exportar para PDF" Height="19px">
                                                                <ClientSideEvents Click="function(s, e) {
var options = {
                format: 'a0',
                landscape: true,
                exportMode: 'all',
                dateRange: 'all',
                fileName: 'Gantt.pdf'
            };       
        
            clientGantt.ExportToPdf(options);
        

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
                                                <dxcp:ASPxButton ID="ASPxButton5" runat="server"  Text="Selecionar" Width="100%" ClientInstanceName="btnSelecionar" OnClick="ASPxButton5_Click">
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
                <tr>
                    <td style="padding-top: 10px">
            <dx:ASPxGantt ID="Gantt" runat="server" Height="600px" Width="100%" ClientInstanceName="clientGantt" EnableViewState="False"
        TasksDataSourceID="TasksDataSource"
        DependenciesDataSourceID="DependenciesDataSource"
        ResourcesDataSourceID="ResourcesDataSource"
        ResourceAssignmentsDataSourceID="ResourceAssignmentsDataSource" OnTaskUpdated="Gantt_TaskUpdated">
        <SettingsTaskList Width="40%" AllowSort="False">
            <Columns>
                <dx:GanttTextColumn FieldName="Tarefa" Caption="Tarefa" Width="360" VisibleIndex="0"/>               
                <dx:GanttDateTimeColumn FieldName="InicioLB" Caption="Início LB" Width="100" DisplayFormat="dd\/MM\/yyyy" VisibleIndex="0" >
<PropertiesDateEdit DisplayFormatString="dd\/MM\/yyyy">
<TimeSectionProperties Visible="False"></TimeSectionProperties>
</PropertiesDateEdit>
                </dx:GanttDateTimeColumn>
                <dx:GanttDateTimeColumn FieldName="TerminoLB" Caption="Término LB" Width="100" DisplayFormat="dd\/MM\/yyyy" VisibleIndex="1" >
<PropertiesDateEdit DisplayFormatString="dd\/MM\/yyyy">
<TimeSectionProperties Visible="False"></TimeSectionProperties>
</PropertiesDateEdit>
                </dx:GanttDateTimeColumn>
                <dx:GanttTextColumn FieldName="PercentualPrevisto" Caption="Previsto" Width="100"  DisplayFormat="{0:n2}%" VisibleIndex="2" >
<PropertiesTextEdit DisplayFormatString="{0:n2}%"></PropertiesTextEdit>
                </dx:GanttTextColumn>
                <dx:GanttProgressBarColumn FieldName="PercentualRealizado" Caption="Realizado" Width="100"  DisplayFormat="{0:n2}" VisibleIndex="3" >
<PropertiesProgressBar DisplayFormatString="{0:n2}"></PropertiesProgressBar>
                </dx:GanttProgressBarColumn>
                <dx:GanttTextColumn FieldName="ValorPesoTarefa" Caption="Peso LB" Width="100" DisplayFormat="{0:n2}" VisibleIndex="4" >
<PropertiesTextEdit DisplayFormatString="{0:n2}"></PropertiesTextEdit>
                </dx:GanttTextColumn>
                <dx:GanttTextColumn FieldName="PercentualPesoTarefa" Caption="% Peso" Width="100"  DisplayFormat="{0:n2}%" VisibleIndex="5" >
<PropertiesTextEdit DisplayFormatString="{0:n2}%"></PropertiesTextEdit>
                </dx:GanttTextColumn>
                <dx:GanttTextColumn FieldName="Trabalho" Caption="Trabalho (h)" Width="100" DisplayFormat="{0:n2}" VisibleIndex="7" >
<PropertiesTextEdit DisplayFormatString="{0:n2}"></PropertiesTextEdit>
                </dx:GanttTextColumn>
                <dx:GanttDateTimeColumn FieldName="Inicio" Caption="Início" Width="100" DisplayFormat="dd\/MM\/yyyy" VisibleIndex="8" >
<PropertiesDateEdit DisplayFormatString="dd\/MM\/yyyy">
<TimeSectionProperties Visible="False"></TimeSectionProperties>
</PropertiesDateEdit>
                </dx:GanttDateTimeColumn>
                <dx:GanttDateTimeColumn FieldName="Termino" Caption="Término" Width="100" DisplayFormat="dd\/MM\/yyyy" VisibleIndex="9" >
<PropertiesDateEdit DisplayFormatString="dd\/MM\/yyyy">
<TimeSectionProperties Visible="False"></TimeSectionProperties>
</PropertiesDateEdit>
                </dx:GanttDateTimeColumn>
                <dx:GanttDateTimeColumn FieldName="TerminoReal" Caption="Término Real" Width="100" DisplayFormat="dd\/MM\/yyyy" VisibleIndex="10" >
<PropertiesDateEdit DisplayFormatString="dd\/MM\/yyyy">
<TimeSectionProperties Visible="False"></TimeSectionProperties>
</PropertiesDateEdit>
                </dx:GanttDateTimeColumn>
                <dx:GanttSpinEditColumn Caption="Duração (d)" DisplayFormat="{0:n0}" FieldName="Duracao" VisibleIndex="6" Width="100px">
                    <PropertiesSpinEdit DisplayFormatString="{0:n0}" NumberFormat="Custom">
                    </PropertiesSpinEdit>
                </dx:GanttSpinEditColumn>
            </Columns>
        </SettingsTaskList>
        <Mappings>
            <Task Key="ID" ParentKey="ParentID" Title="Tarefa" Start="Inicio" End="Termino" Progress="PercentualRealizado"  Color="CorTarefa" />
            <Dependency Key="ID" PredecessorKey="ParentID" SuccessorKey="DependentID" DependencyType="Type" />
            <Resource Key="ID" Name="Name" />
            <ResourceAssignment Key="ID" TaskKey="TaskID" ResourceKey="ResourceID" />
        </Mappings>
                <ClientSideEvents CustomCommand="function(s, e) {
	editaTarefa(s, e);
}" />
        <SettingsGanttView ViewType="Weeks" />

<SettingsToolbar><Items>
<dx:GanttUndoToolbarItem Name="6" BeginGroup="True" GroupName="6" Text="" ToolTip="Undo">
<Image>
<SpriteProperties CheckedCssClass="dxGantt_UndoChecked" DisabledCssClass="dxGantt_UndoDisabled" CssClass="dxGantt_Undo"></SpriteProperties>
</Image>
</dx:GanttUndoToolbarItem>
<dx:GanttRedoToolbarItem Name="7" GroupName="7" Text="" ToolTip="Redo">
<Image>
<SpriteProperties CheckedCssClass="dxGantt_RedoChecked" DisabledCssClass="dxGantt_RedoDisabled" CssClass="dxGantt_Redo"></SpriteProperties>
</Image>
</dx:GanttRedoToolbarItem>
<dx:GanttCollapseAllToolbarItem Name="11" BeginGroup="True" GroupName="11" Text="" ToolTip="Collapse All">
<Image>
<SpriteProperties CheckedCssClass="dxGantt_CollapseChecked" DisabledCssClass="dxGantt_CollapseDisabled" CssClass="dxGantt_Collapse"></SpriteProperties>
</Image>
</dx:GanttCollapseAllToolbarItem>
<dx:GanttExpandAllToolbarItem Name="12" GroupName="12" Text="" ToolTip="Expand All">
<Image>
<SpriteProperties CheckedCssClass="dxGantt_ExpandChecked" DisabledCssClass="dxGantt_ExpandDisabled" CssClass="dxGantt_Expand"></SpriteProperties>
</Image>
</dx:GanttExpandAllToolbarItem>
<dx:GanttAddTaskToolbarItem Name="0" BeginGroup="True" GroupName="0" Text="" ToolTip="Add New Task">
<Image>
<SpriteProperties CheckedCssClass="dxGantt_AddChecked" DisabledCssClass="dxGantt_AddDisabled" CssClass="dxGantt_Add"></SpriteProperties>
</Image>
</dx:GanttAddTaskToolbarItem>
<dx:GanttRemoveTaskToolbarItem Name="2" GroupName="2" Text="" ToolTip="Delete Selected Task">
<Image>
<SpriteProperties CheckedCssClass="dxGantt_DeleteChecked" DisabledCssClass="dxGantt_DeleteDisabled" CssClass="dxGantt_Delete"></SpriteProperties>
</Image>
</dx:GanttRemoveTaskToolbarItem>
<dx:GanttZoomInToolbarItem Name="8" BeginGroup="True" GroupName="8" Text="" ToolTip="Zoom In">
<Image>
<SpriteProperties CheckedCssClass="dxGantt_ZoomInChecked" DisabledCssClass="dxGantt_ZoomInDisabled" CssClass="dxGantt_ZoomIn"></SpriteProperties>
</Image>
</dx:GanttZoomInToolbarItem>
<dx:GanttZoomOutToolbarItem Name="9" GroupName="9" Text="" ToolTip="Zoom Out">
<Image>
<SpriteProperties CheckedCssClass="dxGantt_ZoomOutChecked" DisabledCssClass="dxGantt_ZoomOutDisabled" CssClass="dxGantt_ZoomOut"></SpriteProperties>
</Image>
</dx:GanttZoomOutToolbarItem>
<dx:GanttFullScreenToolbarItem Name="10" BeginGroup="True" GroupName="10" Text="" ToolTip="Full Screen">
<Image>
<SpriteProperties CheckedCssClass="dxGantt_FullScreenChecked" DisabledCssClass="dxGantt_FullScreenDisabled" CssClass="dxGantt_FullScreen"></SpriteProperties>
</Image>
</dx:GanttFullScreenToolbarItem>
<dx:GanttResourceManagerToolbarItem Name="13" BeginGroup="True" GroupName="13" Text="" ToolTip="Resource Manager">
<Image>
<SpriteProperties CheckedCssClass="dxGantt_ResourceManagerChecked" DisabledCssClass="dxGantt_ResourceManagerDisabled" CssClass="dxGantt_ResourceManager"></SpriteProperties>
</Image>
</dx:GanttResourceManagerToolbarItem>
<dx:GanttTaskDetailsToolbarItem Name="4" BeginGroup="True" GroupName="4" Text="" ToolTip="Task Details">
<Image>
<SpriteProperties CheckedCssClass="dxGantt_TaskDetailsChecked" DisabledCssClass="dxGantt_TaskDetailsDisabled" CssClass="dxGantt_TaskDetails"></SpriteProperties>
</Image>
</dx:GanttTaskDetailsToolbarItem>
    <dx:GanttCustomToolbarItem CommandName="btnDetalhesTarefa" Name="btnDetalhesTarefa">
    </dx:GanttCustomToolbarItem>
</Items>
</SettingsToolbar>
    </dx:ASPxGantt>
                    </td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                </tr>
                 <tr>
     <td>                                    <table cellpadding="0" cellspacing="0" class="style4">
                                        <tr>
                                            <td style="padding-left: 10px" class="auto-style1">
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
                                            <td align="left">
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
                                    </table></td>
 </tr>
            </table>
    <asp:ObjectDataSource ID="TasksDataSource" runat="server" DataObjectTypeName="BriskPtf.ClassesBase.Task" TypeName="BriskPtf.ClassesBase.GanttDataProvider" SelectMethod="GetTasks" UpdateMethod="UpdateTask" InsertMethod="InsertTask" DeleteMethod="DeleteTask" OnUpdated="TasksDataSource_Updated"  />
        <dxpc:ASPxPopupControl ID="pcDados" runat="server" ClientInstanceName="pcDados"
            
            HeaderText="Dados da Tarefa"
            Width="631px">
            <SettingsLoadingPanel Enabled="False" />
            <ContentCollection>
                <dxpc:PopupControlContentControl runat="server" SupportsDisabledAttribute="True">
                    <table cellspacing="1" class="style4">
                        <tr>
                            <td>
                                <dxtv:ASPxLabel ID="ASPxLabel15" runat="server" Text="Tarefa:">
                                </dxtv:ASPxLabel>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <dxtv:ASPxTextBox ID="txtStatus0" runat="server" ClientEnabled="False" Width="100%">
                                    <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                    </DisabledStyle>
                                </dxtv:ASPxTextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="style10"></td>
                        </tr>
                        <tr>
                            <td>
                                <dxtv:ASPxPageControl ID="ASPxPageControl1" runat="server" ActiveTabIndex="0" Width="100%">
                                    <TabPages>
                                        <dxtv:TabPage Name="tbDetalhes" Text="Tarefa">
                                            <ContentCollection>
                                                <dxtv:ContentControl runat="server">
                                                    <table cellspacing="1" width="100%">
                                                        <tr>
                                                            <td>
                                                                <table cellspacing="1" class="style4">
                                                                    <tr>
                                                                        <td class="style8">
                                                                            <dxtv:ASPxLabel ID="ASPxLabel21" runat="server" Text="Inicio:">
                                                                            </dxtv:ASPxLabel>
                                                                        </td>
                                                                        <td class="style8">
                                                                            <dxtv:ASPxLabel ID="ASPxLabel26" runat="server" Text="Término:">
                                                                            </dxtv:ASPxLabel>
                                                                        </td>
                                                                        <td>
                                                                            <dxtv:ASPxLabel ID="ASPxLabel27" runat="server" Text="Duração:">
                                                                            </dxtv:ASPxLabel>
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td class="style8" style="padding-right: 10px">
                                                                            <dxtv:ASPxDateEdit ID="dtInicio0" runat="server" ClientInstanceName="dtInicio">
                                                                            </dxtv:ASPxDateEdit>
                                                                        </td>
                                                                        <td class="style8" style="padding-right: 10px">
                                                                            <dxtv:ASPxDateEdit ID="dtInicio1" runat="server" ClientInstanceName="dtInicio">
                                                                            </dxtv:ASPxDateEdit>
                                                                        </td>
                                                                        <td>
                                                                            <dxtv:ASPxTextBox ID="txtDataSolicitacao2" runat="server" ClientEnabled="False" DisplayFormatString="{0:dd/MM/yyyy HH:mm:ss}" Width="100%">
                                                                                <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                                                </DisabledStyle>
                                                                            </dxtv:ASPxTextBox>
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
                                                                <dxtv:ASPxRoundPanel ID="ASPxRoundPanel1" runat="server" HeaderText="Linha de Base" ShowCollapseButton="True" View="GroupBox" Width="100%">
                                                                    <ContentPaddings Padding="5px" PaddingBottom="2px" />
                                                                    <PanelCollection>
                                                                        <dxtv:PanelContent runat="server">
                                                                            <table cellspacing="1" class="style4">
                                                                                <tr>
                                                                                    <td class="style8">
                                                                                        <dxtv:ASPxLabel ID="ASPxLabel28" runat="server" Text="Inicio:">
                                                                                        </dxtv:ASPxLabel>
                                                                                    </td>
                                                                                    <td class="style8">
                                                                                        <dxtv:ASPxLabel ID="ASPxLabel29" runat="server" Text="Término:">
                                                                                        </dxtv:ASPxLabel>
                                                                                    </td>
                                                                                    <td class="auto-style2">
                                                                                        <dxtv:ASPxLabel ID="ASPxLabel30" runat="server" Text="Duração:">
                                                                                        </dxtv:ASPxLabel>
                                                                                    </td>
                                                                                    <td>
                                                                                        <dxtv:ASPxLabel ID="ASPxLabel34" runat="server" Text="Trabalho:">
                                                                                        </dxtv:ASPxLabel>
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td class="style8" style="padding-right: 10px">
                                                                                        <dxtv:ASPxDateEdit ID="dtInicio2" runat="server" ClientInstanceName="dtInicio">
                                                                                        </dxtv:ASPxDateEdit>
                                                                                    </td>
                                                                                    <td class="style8" style="padding-right: 10px">
                                                                                        <dxtv:ASPxDateEdit ID="dtInicio3" runat="server" ClientInstanceName="dtInicio">
                                                                                        </dxtv:ASPxDateEdit>
                                                                                    </td>
                                                                                    <td class="auto-style2" style="padding-right: 10px">
                                                                                        <dxtv:ASPxTextBox ID="txtDataSolicitacao3" runat="server" ClientEnabled="False" DisplayFormatString="{0:dd/MM/yyyy HH:mm:ss}" Width="100%">
                                                                                            <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                                                            </DisabledStyle>
                                                                                        </dxtv:ASPxTextBox>
                                                                                    </td>
                                                                                    <td>
                                                                                        <dxtv:ASPxTextBox ID="txtDataSolicitacao5" runat="server" ClientEnabled="False" DisplayFormatString="{0:dd/MM/yyyy HH:mm:ss}" Width="100%">
                                                                                            <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                                                            </DisabledStyle>
                                                                                        </dxtv:ASPxTextBox>
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </dxtv:PanelContent>
                                                                    </PanelCollection>
                                                                </dxtv:ASPxRoundPanel>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="style7"></td>
                                                        </tr>
                                                        <tr>
                                                            <td>
                                                                <dxtv:ASPxRoundPanel ID="ASPxRoundPanel2" runat="server" HeaderText="Realizado" ShowCollapseButton="True" View="GroupBox" Width="100%">
                                                                    <ContentPaddings Padding="5px" PaddingBottom="2px" />
                                                                    <PanelCollection>
                                                                        <dxtv:PanelContent runat="server">
                                                                            <table cellspacing="1" class="style4">
                                                                                <tr>
                                                                                    <td class="style8">
                                                                                        <dxtv:ASPxLabel ID="ASPxLabel35" runat="server" Text="Inicio:">
                                                                                        </dxtv:ASPxLabel>
                                                                                    </td>
                                                                                    <td class="style8">
                                                                                        <dxtv:ASPxLabel ID="ASPxLabel36" runat="server" Text="Término:">
                                                                                        </dxtv:ASPxLabel>
                                                                                    </td>
                                                                                    <td class="auto-style2">
                                                                                        <dxtv:ASPxLabel ID="ASPxLabel37" runat="server" Text="Duração:">
                                                                                        </dxtv:ASPxLabel>
                                                                                    </td>
                                                                                    <td>
                                                                                        <dxtv:ASPxLabel ID="ASPxLabel38" runat="server" Text="% Concluído:">
                                                                                        </dxtv:ASPxLabel>
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td class="style8" style="padding-right: 10px">
                                                                                        <dxtv:ASPxDateEdit ID="dtInicio6" runat="server" ClientInstanceName="dtInicio">
                                                                                        </dxtv:ASPxDateEdit>
                                                                                    </td>
                                                                                    <td class="style8" style="padding-right: 10px">
                                                                                        <dxtv:ASPxDateEdit ID="dtInicio7" runat="server" ClientInstanceName="dtInicio">
                                                                                        </dxtv:ASPxDateEdit>
                                                                                    </td>
                                                                                    <td class="auto-style2" style="padding-right: 10px">
                                                                                        <dxtv:ASPxTextBox ID="txtDataSolicitacao6" runat="server" ClientEnabled="False" DisplayFormatString="{0:dd/MM/yyyy HH:mm:ss}" Width="100%">
                                                                                            <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                                                            </DisabledStyle>
                                                                                        </dxtv:ASPxTextBox>
                                                                                    </td>
                                                                                    <td>
                                                                                        <dxtv:ASPxTextBox ID="txtDataSolicitacao7" runat="server" ClientEnabled="False" DisplayFormatString="{0:dd/MM/yyyy HH:mm:ss}" Width="100%">
                                                                                            <DisabledStyle BackColor="#EBEBEB" ForeColor="Black">
                                                                                            </DisabledStyle>
                                                                                        </dxtv:ASPxTextBox>
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </dxtv:PanelContent>
                                                                    </PanelCollection>
                                                                </dxtv:ASPxRoundPanel>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </dxtv:ContentControl>
                                            </ContentCollection>
                                        </dxtv:TabPage>
                                        <dxtv:TabPage Name="tbPredecessoras" Text="Predecessoras">
                                            <ContentCollection>
                                                <dxtv:ContentControl runat="server">
                                                </dxtv:ContentControl>
                                            </ContentCollection>
                                        </dxtv:TabPage>
                                        <dxtv:TabPage Text="Sucessoras">
                                            <ContentCollection>
                                                <dxtv:ContentControl runat="server">
                                                </dxtv:ContentControl>
                                            </ContentCollection>
                                        </dxtv:TabPage>
                                        <dxtv:TabPage Text="Recursos">
                                            <ContentCollection>
                                                <dxtv:ContentControl runat="server">
                                                </dxtv:ContentControl>
                                            </ContentCollection>
                                        </dxtv:TabPage>
                                        <dxtv:TabPage Text="Custos">
                                            <ContentCollection>
                                                <dxtv:ContentControl runat="server">
                                                </dxtv:ContentControl>
                                            </ContentCollection>
                                        </dxtv:TabPage>
                                        <dxtv:TabPage Text="Anotações">
                                            <ContentCollection>
                                                <dxtv:ContentControl runat="server">
                                                </dxtv:ContentControl>
                                            </ContentCollection>
                                        </dxtv:TabPage>
                                    </TabPages>
                                </dxtv:ASPxPageControl>
                            </td>
                        </tr>
                        <tr>
                            <td class="style10"></td>
                        </tr>
                        <tr>
                            <td align="right">
                                <table cellspacing="1">
                                    <tr>
                                        <td style="padding-right: 10px">
                                            <dxtv:ASPxButton ID="btnSalvar" runat="server" Text="Salvar" Width="100px">
                                            </dxtv:ASPxButton>
                                        </td>
                                        <td>
                                            <dxtv:ASPxButton ID="btnCancelar" runat="server" Text="Cancelar" Width="100px">
                                            </dxtv:ASPxButton>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </dxpc:PopupControlContentControl>
            </ContentCollection>
        </dxpc:ASPxPopupControl>
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
    <asp:ObjectDataSource ID="DependenciesDataSource" runat="server" DataObjectTypeName="BriskPtf.ClassesBase.Dependency" TypeName="BriskPtf.ClassesBase.GanttDataProvider" SelectMethod="GetDependencies" InsertMethod="InsertDependency" DeleteMethod="DeleteDependency" />
    <asp:ObjectDataSource ID="ResourcesDataSource" runat="server" DataObjectTypeName="BriskPtf.ClassesBase.Resource" TypeName="BriskPtf.ClassesBase.GanttDataProvider" SelectMethod="GetResources" UpdateMethod="UpdateResource" InsertMethod="InsertResource" DeleteMethod="DeleteResource" />
    <asp:ObjectDataSource ID="ResourceAssignmentsDataSource" runat="server" DataObjectTypeName="BriskPtf.ClassesBase.ResourceAssignment" TypeName="BriskPtf.ClassesBase.GanttDataProvider" SelectMethod="GetResourceAssignments" InsertMethod="InsertResourceAssignment" DeleteMethod="DeleteResourceAssignment" />
        </div>        
    </form>
</body>
</html>
