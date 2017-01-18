/// <reference path="../../../types-gtanetwork/index.d.ts" />

const VehicleFuelStatusKey = "FUEL_STATUS";
const UsePumpEvent = "USE_PUMP";
const DisconnectFromPumpEvent = "DISCONNECT_FROM_PUMP";
const UsePumpControl = Enums.Controls.Context;

class Imaging {
    private static fuelGaugeBackSize = new Size(200, 126);
    private static fuelGaugeBackPoint: Point = null;
    private static fuelGaugeNeedleSize = new Size(200, 200);
    private static fuelGaugeNeedlePoint: Point = null;
    private static gasPumpIconSize = new Size(48, 48);

    public static Initialize() {
        var screenResolution = API.getScreenResolutionMantainRatio();

        Imaging.fuelGaugeBackPoint = new Point(
            screenResolution.Width - Imaging.fuelGaugeBackSize.Width - 30,
            screenResolution.Height - Imaging.fuelGaugeBackSize.Height);

        Imaging.fuelGaugeNeedlePoint = new Point(
            screenResolution.Width - Imaging.fuelGaugeNeedleSize.Width - 30,
            screenResolution.Height - (Imaging.fuelGaugeNeedleSize.Height / 2) - 30);
    }

    public static RenderGauge(fuelStatus: VehicleFuelStatus) {
        API.dxDrawTexture(
            "Client/UI/img/gauge_back.png",
            Imaging.fuelGaugeBackPoint,
            Imaging.fuelGaugeBackSize,
            0);

        var tankPercentage = fuelStatus.CurrentFuel / fuelStatus.FuelCapacity;

        // Full = 72
        // Empty = -72
        API.dxDrawTexture(
            "Client/UI/img/gauge_needle.png",
            Imaging.fuelGaugeNeedlePoint,
            Imaging.fuelGaugeNeedleSize,
            144 * tankPercentage - 72);
    }
    
    public static RenderGasPumpIcon(entityPosition: Vector3) {
        entityPosition.Z += 1;
        var worldToScreen = API.worldToScreenMantainRatio(entityPosition);
        var drawPoint = new Point(Math.round(worldToScreen.X - Imaging.gasPumpIconSize.Width / 2), Math.round(worldToScreen.Y - Imaging.gasPumpIconSize.Height / 2));

        API.dxDrawTexture("Client/UI/img/gas_pump_icon.png", drawPoint, Imaging.gasPumpIconSize, 0);
    }
}

const enum FuelTypes {
    Gasoline = 1,
    Kerosene = 2,
    Electricity = 3
}

var GasPumpModels = [
    1339433404,
    1933174915,
    -2007231801,
    -462817101,
    -469694731,
    1694452750
];

class VehicleFuelStatus {
    public CurrentFuel: number;
    public FuelCapacity: number;
    public ConsumptionMultiplier: number;
    public FuelType: FuelTypes;
}

function Vector3Lerp(start: Vector3, end: Vector3, fraction: number) {
    return new Vector3(
        (start.X + (end.X - start.X) * fraction),
        (start.Y + (end.Y - start.Y) * fraction),
        (start.Z + (end.Z - start.Z) * fraction)
    );
}

function getAimPoint() {
    var resolution = API.getScreenResolutionMantainRatio();
    return API.screenToWorldMantainRatio(new PointF(resolution.Width / 2, resolution.Height / 2));
}

function GetLookingAtGasPumpEntity(): GTANetwork.Util.LocalHandle {
    var startPoint = API.getGameplayCamPos();
    var aimPoint = getAimPoint();

    startPoint.Add(aimPoint);

    var endPoint = Vector3Lerp(startPoint, aimPoint, 1.1);
    var rayCast = API.createRaycast(startPoint, endPoint, Enums.IntersectOptions.Everything, null);

    if (!rayCast.didHitEntity) return null;

    var hitEntityHandle = rayCast.hitEntity;
    var entityModel = API.getEntityModel(hitEntityHandle);

    if (GasPumpModels.indexOf(entityModel) == -1) return null;

    if (API.getEntityPosition(hitEntityHandle).DistanceTo(API.getEntityPosition(API.getLocalPlayer())) > 3) return null;

    return hitEntityHandle;
}