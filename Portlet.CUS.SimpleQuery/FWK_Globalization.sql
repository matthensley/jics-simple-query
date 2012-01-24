use ICS_NET
GO

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_QUERY_TITLE_LABEL' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_QUERY_TITLE_LABEL', 'En','Query Title',null);

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_QUERY_TITLE_DESC' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_QUERY_TITLE_DESC', 'En','The title of the query to be displayed at the top of the results page. If left blank, no title will appear above the results.',null);

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_DISP_RESULT_HEADINGS_LABEL' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_DISP_RESULT_HEADINGS_LABEL', 'En','Show Column Headings in Query Results',null); 

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_DISP_RESULT_HEADINGS_DESC' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_DISP_RESULT_HEADINGS_DESC', 'En','If checked, the query results will display column headings using the selected field names or aliases defined in the query.',null);

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_DISP_ALT_ROW_COLOR_LABEL' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_DISP_ALT_ROW_COLOR_LABEL', 'En','Use Alternating Row Color in Results',null); 

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_DISP_ALT_ROW_COLOR_DESC' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_DISP_ALT_ROW_COLOR_DESC', 'En','If checked, the query results will display even-numbered rows with a background color to match the border color.',null);

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_DISP_GRID_BORDERS_LABEL' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_DISP_GRID_BORDERS_LABEL', 'En','Show Borders in Results Grid',null); 

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_DISP_GRID_BORDERS_DESC' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_DISP_GRID_BORDERS_DESC', 'En','If checked, the query results will display borders around and between the results grid cells using the color specified in the portlet template.',null);

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_DISP_RESULT_MIN_LABEL' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_DISP_RESULT_MIN_LABEL', 'En','Show Query Results in Minimized View',null);

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_DISP_RESULT_MIN_DESC' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_DISP_RESULT_MIN_DESC', 'En','If checked, the portlet will perform the query and present the results every time it is viewed, both when it is one of several portlets on the page (minimized view) and when it is the only portlet visible on the page (maximized view). <br />If left unchecked, the query results will only display after the user maximizes the portlet either by clicking on a link presented in minimized view or by clicking on the portlet title.',null);

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_VIEW_RESULTS_LINK_TEXT_LABEL' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_VIEW_RESULTS_LINK_TEXT_LABEL', 'En', 'View Results Link Title',null);
    
IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_VIEW_RESULTS_LINK_TEXT_DESC' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_VIEW_RESULTS_LINK_TEXT_DESC', 'En', 'The text of the link to be shown in minimized view if that option is chosen above. If no text is entered, the default text View Results will be used.',null);

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_SUMMARY_LABEL' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_SUMMARY_LABEL', 'En','Query Summary Description',null);

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_SUMMARY_DESC' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_SUMMARY_DESC', 'En','An optional description to be displayed only in the minimized view of the portlet along with a link to show the results. This will only show if Show Query Results in Minimized View is checked.',null);

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_OPR_CANADMINQUERIES' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_OPR_CANADMINQUERIES', 'En','Can Create SELECT Queries', null);

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_OPR_CANADMINADVQUERIES' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_OPR_CANADMINADVQUERIES', 'En','Can Create Any (DEL, UPD, INS, SEL, EXEC) Queries', null);

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_DISP_EXCEL_EXPORT_LABEL' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_DISP_EXCEL_EXPORT_LABEL', 'En','Display Export Link', null);

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_DISP_EXCEL_EXPORT_DESC' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_DISP_EXCEL_EXPORT_DESC', 'En','Show a link above the query results which, when clicked, will create a file containing the query results.  In the case of Excel format, this is actually an HTML table  in a file with a .xls extension.  XML and CSV formats yield a file with extension .xml and .csv, respectively.  The file name is derived from the Query Title (see above), if there is one.', null);
    
IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_CELL_PADDING_LABEL' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_CELL_PADDING_LABEL', 'En','Results Grid Cell Padding (pixels)', null);

IF NOT EXISTS(SELECT Text_Key FROM FWK_Globalization WHERE Text_Key='CUS_SIMPLEQUERY_CELL_PADDING_DESC' AND Language_Code='En')
    INSERT INTO FWK_Globalization (Text_Key, Language_Code, Text_Value,Text_Custom_Value)
    VALUES ('CUS_SIMPLEQUERY_CELL_PADDING_DESC', 'En','The space between the contents of each cell of the results grid and the boundary of that cell. Value should be an integer representing a number of pixels.', null);
    
