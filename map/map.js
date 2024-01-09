function createMap(divId, tilePath) {
	var mapExtent = [0, -12032, 13312, 0];
	var mapMinZoom = 0;
	var mapMaxZoom = 6;
	var mapMinResolution = Math.pow(2, mapMaxZoom);
	var crs = L.CRS.Simple;
	crs.transformation = new L.Transformation(1, -mapExtent[0], -1, mapExtent[3]);
	crs.scale=function(zoom){return Math.pow(2, zoom)/mapMinResolution;};
	crs.zoom=function(scale){return Math.log(scale*mapMinResolution)/Math.LN2;};
	var map = new L.Map(divId,{maxZoom: mapMaxZoom*2,minZoom: mapMinZoom,crs: crs});
	var layer = L.tileLayer(tilePath + '{z}/{x}/{y}.png',{minZoom:mapMinZoom,maxNativeZoom:mapMaxZoom,maxZoom:mapMaxZoom*2,noWrap:true,tms:false}).addTo(map);
	map.fitBounds([crs.unproject(L.point(mapExtent[2], mapExtent[3])),crs.unproject(L.point(mapExtent[0], mapExtent[1]))]);
}