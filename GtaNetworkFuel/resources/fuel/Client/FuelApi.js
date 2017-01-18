/// <reference path="../../../types-gtanetwork/index.d.ts" />
var VehicleFuelStatusKey = "FUEL_STATUS";
var UsePumpEvent = "USE_PUMP";
var DisconnectFromPumpEvent = "DISCONNECT_FROM_PUMP";
var UsePumpControl = 51 /* Context */;
var Imaging = (function () {
    function Imaging() {
    }
    Imaging.Initialize = function () {
        var screenResolution = API.getScreenResolutionMantainRatio();
        Imaging.fuelGaugeBackPoint = new Point(screenResolution.Width - Imaging.fuelGaugeBackSize.Width - 30, screenResolution.Height - Imaging.fuelGaugeBackSize.Height);
        Imaging.fuelGaugeNeedlePoint = new Point(screenResolution.Width - Imaging.fuelGaugeNeedleSize.Width - 30, screenResolution.Height - (Imaging.fuelGaugeNeedleSize.Height / 2) - 30);
    };
    Imaging.RenderGauge = function (fuelStatus) {
        API.dxDrawTexture("Client/UI/img/gauge_back.png", Imaging.fuelGaugeBackPoint, Imaging.fuelGaugeBackSize, 0);
        var tankPercentage = fuelStatus.CurrentFuel / fuelStatus.FuelCapacity;
        // Full = 72
        // Empty = -72
        API.dxDrawTexture("Client/UI/img/gauge_needle.png", Imaging.fuelGaugeNeedlePoint, Imaging.fuelGaugeNeedleSize, 144 * tankPercentage - 72);
    };
    Imaging.RenderGasPumpIcon = function (entityPosition) {
        entityPosition.Z += 1;
        var worldToScreen = API.worldToScreenMantainRatio(entityPosition);
        var drawPoint = new Point(Math.round(worldToScreen.X - Imaging.gasPumpIconSize.Width / 2), Math.round(worldToScreen.Y - Imaging.gasPumpIconSize.Height / 2));
        API.dxDrawTexture("Client/UI/img/gas_pump_icon.png", drawPoint, Imaging.gasPumpIconSize, 0);
    };
    Imaging.fuelGaugeBackSize = new Size(200, 126);
    Imaging.fuelGaugeBackPoint = null;
    Imaging.fuelGaugeNeedleSize = new Size(200, 200);
    Imaging.fuelGaugeNeedlePoint = null;
    Imaging.gasPumpIconSize = new Size(48, 48);
    return Imaging;
}());
var GasPumpModels = [
    1339433404,
    1933174915,
    -2007231801,
    -462817101,
    -469694731,
    1694452750
];
var VehicleFuelStatus = (function () {
    function VehicleFuelStatus() {
    }
    return VehicleFuelStatus;
}());
function Vector3Lerp(start, end, fraction) {
    return new Vector3((start.X + (end.X - start.X) * fraction), (start.Y + (end.Y - start.Y) * fraction), (start.Z + (end.Z - start.Z) * fraction));
}
function getAimPoint() {
    var resolution = API.getScreenResolutionMantainRatio();
    return API.screenToWorldMantainRatio(new PointF(resolution.Width / 2, resolution.Height / 2));
}
function GetLookingAtGasPumpEntity() {
    var startPoint = API.getGameplayCamPos();
    var aimPoint = getAimPoint();
    startPoint.Add(aimPoint);
    var endPoint = Vector3Lerp(startPoint, aimPoint, 1.1);
    var rayCast = API.createRaycast(startPoint, endPoint, -1 /* Everything */, null);
    if (!rayCast.didHitEntity)
        return null;
    var hitEntityHandle = rayCast.hitEntity;
    var entityModel = API.getEntityModel(hitEntityHandle);
    if (GasPumpModels.indexOf(entityModel) == -1)
        return null;
    if (API.getEntityPosition(hitEntityHandle).DistanceTo(API.getEntityPosition(API.getLocalPlayer())) > 3)
        return null;
    return hitEntityHandle;
}
