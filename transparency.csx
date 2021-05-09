// '2021-05-09 / B.Agullo / 
// this script creates a transparency measure based on a selected color measure 

//select a color measure (conditional or otherwise) and a new measure 
//with the same name + " transparent" will be created

//to modify the transparency use a selector on the numeric gradient on the hidden table created for it


//
// ----- do not modify script below this line -----
//

if (Selected.Measures.Count == 0) {
  Error("Select one or more measures");
  return;
}


string calcTableExpression = 
"VAR numericValueColumn =" + 
 "    SELECTCOLUMNS ( GENERATESERIES ( 0, 255, 1 ), \"Numeric Value\", [Value] )" + 
 "VAR result =" + 
 "    GENERATE (" + 
 "        numericValueColumn," + 
 "        VAR numericValue = [Numeric Value]" + 
 "        VAR firstHexNum =" + 
 "            INT ( DIVIDE ( numericValue, 16 ) )" + 
 "        VAR firstHexLetter =" + 
 "            IF (" + 
 "                firstHexNum <= 9," + 
 "                FORMAT ( firstHexNum, \"0\" )," + 
 "                UNICHAR ( 65 + ( firstHexNum - 10 ) )" + 
 "            )" + 
 "        VAR secondHexNum =" + 
 "            INT ( ( DIVIDE ( numericValue, 16 ) - firstHexNum ) * 16 )" + 
 "        VAR secondHexLetter =" + 
 "            IF (" + 
 "                secondHexNum <= 9," + 
 "                FORMAT ( secondHexNum, \"0\" )," + 
 "                UNICHAR ( 65 + ( secondHexNum - 10 ) )" + 
 "            )" + 
 "        RETURN" + 
 "            ROW ( \"Hex Value\", firstHexLetter & secondHexLetter , \"Percent\" ,numericValue / 255 )" + 
 "    )" + 
 "RETURN" + 
 "    result";

 
 foreach(var m in Selected.Measures) {
     
    string calcTableName = "Transparency of " + m.Name; 
    string transparentMeasureName = m.Name + " transparent"; 
    
    foreach(var n in Model.AllMeasures) {
        if (n.Name == transparentMeasureName) {
            Error("This measure name already exists in table " + n.Table.Name + ". Either rename the existing measure or choose a different name for the measure in your Calculation Group.");
            return;
        };
    };
    
    if (!Model.Tables.Contains(calcTableName)) {
        var transparencyTable =  Model.AddCalculatedTable(calcTableName,calcTableExpression);
        transparencyTable.FormatDax(); 
        transparencyTable.Description = "Table to define transparency of " + transparentMeasureName;
        transparencyTable.IsHidden = true;
        
        //this marks it as a "as-if" parameter which enables single value by slicer
        transparencyTable.Columns[Percent].SetExtendedProperty("ParameterMetadata", "{\"version\":0}", ExtendedPropertyType.Json);
        
    }; 
    
    string transparentMeasureExpression = "[" + m.Name + "] & FORMAT(SELECTEDVALUE('" + calcTableName +"'[Hex Value],\"FF\"),\"@\")";
    
    var transparentMeasure = m.Table.AddMeasure(transparentMeasureName,transparentMeasureExpression);
    transparentMeasure.FormatDax(); 
    transparentMeasure.Description = m.Name + " with a degree of transparency defined by the selected value on " + calcTableName + " table";
    
    
    
};
 
