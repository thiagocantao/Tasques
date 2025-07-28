<%@ Page Title="" Language="C#" MasterPageFile="~/novaCdis.master" AutoEventWireup="true"
    CodeBehind="analiseDeFluxoPorPeriodo.aspx.cs" Inherits="BriskPtf._Projetos._Projetos_analiseDeFluxoPorPeriodo" %>

<asp:Content ID="Content1" ContentPlaceHolderID="AreaTrabalho" runat="Server">
    <table border="0" cellpadding="0" cellspacing="0" style="width: 100%; background-image: url(../imagens/titulo/back_Titulo_Desktop.gif);">
        <tr height="26px">
            <td valign="middle" style="padding-left: 10px">
                <asp:Label ID="lblTitulo" runat="server" Font-Bold="True"
                    Font-Overline="False" Font-Strikeout="False" Text="Fluxos associados diretamente ao projeto"
                    EnableViewState="False"></asp:Label>
            </td>
            <td align="left" valign="middle">
            </td>
        </tr>
    </table>
    <table cellpadding="0" cellspacing="0" enableviewstate="false" style="width:100%">
        <tr>
            <td>
                <table width="100%" style="padding-left: 5px; padding-right: 10px">
                    <tr>
                        <td style="padding-left: 10px;" class="auto-style2">
                            <dxe:ASPxLabel ID="ASPxLabel3" runat="server" Text="Carteira"
                               >
                            </dxe:ASPxLabel>
                        </td>
                        <td class="auto-style3">
                            <dxe:ASPxLabel ID="ASPxLabel4" runat="server" 
                                Text="De:">
                            </dxe:ASPxLabel>
                        </td>
                        <td class="auto-style3">
                            <dxe:ASPxLabel ID="ASPxLabel5" runat="server" 
                                Text="Até:">
                            </dxe:ASPxLabel>
                        </td>
                        <td width="120" class="auto-style3">
                            </td>
                        <td class="auto-style3">
                            </td>
                    </tr>
                    <tr>
                        <td style="width:60%; padding-left: 10px;">
                            <dxe:ASPxComboBox ID="cmbCarteiras" ClientInstanceName="cmbCarteiras" runat="server"
                                Width="100%" >
                            </dxe:ASPxComboBox>
                        </td>
                        <td style="width: 10%">
                            <dxe:ASPxDateEdit ID="dtDe" runat="server" ClientInstanceName="dtDe" 
                                 Width="100%">
                                <CalendarProperties>
                                    <DayHeaderStyle  />
                                    <WeekNumberStyle >
                                    </WeekNumberStyle>
                                    <DayStyle  />
                                    <DaySelectedStyle >
                                    </DaySelectedStyle>
                                    <DayOtherMonthStyle >
                                    </DayOtherMonthStyle>
                                    <DayWeekendStyle >
                                    </DayWeekendStyle>
                                    <DayOutOfRangeStyle >
                                    </DayOutOfRangeStyle>
                                    <TodayStyle >
                                    </TodayStyle>
                                    <ButtonStyle >
                                    </ButtonStyle>
                                    <HeaderStyle  />
                                    <FooterStyle  />
                                    <FastNavStyle >
                                    </FastNavStyle>
                                    <FastNavMonthAreaStyle >
                                    </FastNavMonthAreaStyle>
                                    <FastNavYearAreaStyle >
                                    </FastNavYearAreaStyle>
                                    <FastNavMonthStyle >
                                    </FastNavMonthStyle>
                                    <FastNavYearStyle >
                                    </FastNavYearStyle>
                                    <FastNavFooterStyle >
                                    </FastNavFooterStyle>
                                    <Style >
                                    </Style>
                                </CalendarProperties>
                            </dxe:ASPxDateEdit>
                        </td>
                        <td style="width: 10%">
                            <dxe:ASPxDateEdit ID="dteAte" runat="server" ClientInstanceName="dteAte" 
                                 Width="100%">
                                <CalendarProperties>
                                    <DayHeaderStyle  />
                                    <WeekNumberStyle >
                                    </WeekNumberStyle>
                                    <DayStyle  />
                                    <DaySelectedStyle >
                                    </DaySelectedStyle>
                                    <DayOtherMonthStyle >
                                    </DayOtherMonthStyle>
                                    <DayWeekendStyle >
                                    </DayWeekendStyle>
                                    <DayOutOfRangeStyle >
                                    </DayOutOfRangeStyle>
                                    <TodayStyle >
                                    </TodayStyle>
                                    <ButtonStyle >
                                    </ButtonStyle>
                                    <HeaderStyle  />
                                    <FooterStyle  />
                                    <FastNavStyle >
                                    </FastNavStyle>
                                    <FastNavMonthAreaStyle >
                                    </FastNavMonthAreaStyle>
                                    <FastNavYearAreaStyle >
                                    </FastNavYearAreaStyle>
                                    <FastNavMonthStyle >
                                    </FastNavMonthStyle>
                                    <FastNavYearStyle >
                                    </FastNavYearStyle>
                                    <FastNavFooterStyle >
                                    </FastNavFooterStyle>
                                    <Style >
                                    </Style>
                                </CalendarProperties>
                            </dxe:ASPxDateEdit>
                        </td>
                        <td style="width:10%">
                            <dxe:ASPxButton ID="btnSelecionar" runat="server" Text="Selecionar"
                               >
                            </dxe:ASPxButton>
                        </td>
                        <td align="right" style="width:10%; padding-right: 10px;">
                            <dxe:ASPxButton ID="btnExportarExcel" runat="server" OnClick="btnExportarExcel_Click"
                                Text="Exportar para Excel" Width="150px" >
                            </dxe:ASPxButton>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td style="padding-left: 10px">
                <dxwgv:ASPxGridView ID="gvDados" runat="server" AutoGenerateColumns="False" KeyFieldName="CodigoWorkflow;CodigoInstanciaWf"
                    OnDetailRowExpandedChanged="gvDados_DetailRowExpandedChanged" Width="99%" OnDetailRowGetButtonVisibility="gvDados_DetailRowGetButtonVisibility"
                    >
                    <ClientSideEvents CustomButtonClick="function(s, e) {
    if(&quot;btnResumo&quot; == e.buttonID)
	{
            s.GetRowValues(e.visibleIndex,&quot;CodigoInstanciaWf;CodigoWorkflow;NomeFluxo&quot;,MontaCamposFormulario);
            e.processOnServer = false;
	}
}" />
                    <TotalSummary>
                        <dxwgv:ASPxSummaryItem FieldName="NomeProjeto" ShowInColumn="Projeto" SummaryType="Count"
                            DisplayFormat="Quantidade de registros: {0}" />
                    </TotalSummary>
                    <Columns>
                        <dxwgv:GridViewCommandColumn VisibleIndex="0" ButtonRenderMode="Image" Width="40px" Caption=" ">
                            <CustomButtons>
                                <dxwgv:GridViewCommandColumnCustomButton ID="btnResumo" Text="Imprime Resumo de Tramitação do Processo">
                                    <Image AlternateText="Resumo do Processo" Url="~/imagens/botoes/btnPDF.png">
                                    </Image>
                                </dxwgv:GridViewCommandColumnCustomButton>
                            </CustomButtons>
                            <CellStyle HorizontalAlign="Center">
                            </CellStyle>
                        </dxwgv:GridViewCommandColumn>
                        <dxwgv:GridViewDataTextColumn FieldName="CodigoCarteira" ReadOnly="True" Visible="False"
                            VisibleIndex="11">
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn FieldName="CodigoProjeto" ReadOnly="True" Visible="False"
                            VisibleIndex="13">
                            <EditFormSettings Visible="False" />
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn FieldName="NomeProjeto" VisibleIndex="5" Caption="Projeto">
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn FieldName="NomeUnidadeNegocio" VisibleIndex="4" Caption="Unidade">
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn FieldName="NomeFluxo" VisibleIndex="6" Caption="Fluxo">
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn FieldName="EtapaAtual" VisibleIndex="7" Caption="Etapa atual">
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn FieldName="QtdeAnexos" VisibleIndex="1" Caption="Anexos"
                            Width="50px">
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn FieldName="CodigoWorkflow" ReadOnly="True" Visible="False"
                            VisibleIndex="14">
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn FieldName="CodigoInstanciaWf" ReadOnly="True" Visible="False"
                            VisibleIndex="16">
                            <EditFormSettings Visible="False" />
                            <EditFormSettings Visible="False" />
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn FieldName="Status" ReadOnly="True" VisibleIndex="2"
                            Width="100px">
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn Caption="Responsável pelo Fluxo" FieldName="nomeResponsavelFluxo"
                            VisibleIndex="8">
                            <HeaderStyle Wrap="True" />
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn Caption="Término" FieldName="DataTerminoInstancia"
                            VisibleIndex="10" Width="80px">
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn Caption="Ano Término 1º Etapa" FieldName="AnoInicio"
                            VisibleIndex="12">
                            <HeaderStyle Wrap="True" />
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn Caption="Mes Término 1º Etapa" FieldName="MesInicio"
                            VisibleIndex="15">
                            <HeaderStyle Wrap="True" />
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn Caption="Unidade Superior" FieldName="NomeUnidadeNegocioSuperior"
                            VisibleIndex="3">
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn Caption="Término 1º Etapa" FieldName="DataInicioInstancia"
                            VisibleIndex="9">
                            <HeaderStyle Wrap="True" />
                        </dxwgv:GridViewDataTextColumn>
                    </Columns>
                    <SettingsPager PageSize="50">
                    </SettingsPager>
                    <Settings ShowGroupPanel="True" ShowFilterRow="True" ShowFooter="True" VerticalScrollBarMode="Visible" />
                    <SettingsDetail ShowDetailRow="True" AllowOnlyOneMasterRowExpanded="True" ExportMode="All" />
                    <Templates>
                        <DetailRow>
                            <dxwgv:ASPxGridView ID="gvAnexos" runat="server" AutoGenerateColumns="False" DataSourceID="dsAnexos"
                                KeyFieldName="CodigoAnexo">
                                <Columns>
                                    <dxwgv:GridViewDataTextColumn FieldName="CodigoFormulario" VisibleIndex="4" ShowInCustomizationForm="True"
                                        Visible="False">
                                    </dxwgv:GridViewDataTextColumn>
                                    <dxwgv:GridViewDataTextColumn FieldName="CodigoAnexo" VisibleIndex="5" Visible="False">
                                    </dxwgv:GridViewDataTextColumn>
                                    <dxwgv:GridViewDataTextColumn VisibleIndex="1" FieldName="CodigoAnexo" Caption=" ">
                                        <DataItemTemplate>
                                            <dxe:ASPxButton ID="btnDownLoad" runat="server" ClientInstanceName="<%# getNomeBotalDownload() %>"
                                                AutoPostBack="False" Height="16px" ImageSpacing="0px" OnClick="btnDownLoad_Click"
                                                ToolTip="Visualizar o arquivo" Width="16px" Wrap="False">
                                                <Image Url="~/imagens/anexo/download.png" />
                                                <FocusRectPaddings Padding="0px" />
                                                <FocusRectBorder BorderColor="Transparent" BorderStyle="None" />
                                                <Border BorderWidth="0px" />
                                            </dxe:ASPxButton>
                                        </DataItemTemplate>
                                    </dxwgv:GridViewDataTextColumn>
                                    <dxwgv:GridViewDataTextColumn FieldName="DescricaoAnexo" VisibleIndex="2" ShowInCustomizationForm="True"
                                        Caption="Descrição">
                                    </dxwgv:GridViewDataTextColumn>
                                    <dxwgv:GridViewDataTextColumn FieldName="Nome" VisibleIndex="3" Caption="Nome do arquivo">
                                    </dxwgv:GridViewDataTextColumn>
                                </Columns>
                                <SettingsBehavior AllowDragDrop="False" AllowSort="False" />
                                <SettingsPager Visible="False" Mode="ShowAllRecords">
                                </SettingsPager>
                            </dxwgv:ASPxGridView>
                        </DetailRow>
                    </Templates>
                </dxwgv:ASPxGridView>                
            </td>
        </tr>
    </table>
    <dxwgv:ASPxGridViewExporter ID="gvExporter" runat="server" GridViewID="gvDados" 
                    onrenderbrick="gvExporter_RenderBrick">
                </dxwgv:ASPxGridViewExporter>
    <asp:SqlDataSource ID="dsAnexos" runat="server" ConnectionString="<%$ ConnectionStrings:dbcdispsConnectionString %>"
                    SelectCommand="
select distinct CodigoFormulario, aa.CodigoAnexo, a.DescricaoAnexo, a.Nome
  from FormulariosInstanciasWorkflows AS fiw inner JOIN
              AnexoAssociacao AS aa ON (aa.CodigoObjetoAssociado = fiw.CodigoFormulario
                                   AND aa.CodigoTipoAssociacao = (select CodigoTipoAssociacao FROM TipoAssociacao WHERE IniciaisTipoAssociacao = 'FO')) inner join
              Anexo a on a.CodigoAnexo = aa.CodigoAnexo                                                                                                      
where fiw.CodigoWorkflow = @CodigoWorkflow
  AND fiw.CodigoInstanciaWf = @CodigoInstanciaWf">
                    <SelectParameters>
                        <asp:Parameter Name="CodigoWorkflow" />
                        <asp:Parameter Name="CodigoInstanciaWf" />
                    </SelectParameters>
                </asp:SqlDataSource>
</asp:Content>
<asp:Content ID="Content2" runat="server" contentplaceholderid="HeadContent">
    <style type="text/css">
        .auto-style2 {
            width: 60%;
            height: 18px;
        }
        .auto-style3 {
            width: 10%;
            height: 18px;
        }
    </style>
</asp:Content>

