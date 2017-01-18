/// <reference path="../../../types-gtanetwork/index.d.ts" />
/// <reference path="FuelApi.ts" />

var localPlayer: GTANetwork.Util.LocalHandle = null;
var currentlyDrivingVehicle: GTANetwork.Util.LocalHandle = null;
var currentVehicleFuelStatus: VehicleFuelStatus;

API.onResourceStart.connect(() => {
    // Set local variables
    localPlayer = API.getLocalPlayer();

    // Hook events
    API.onPlayerEnterVehicle.connect(playerEnterVehicle);
    API.onPlayerExitVehicle.connect(playerExitVehicle);
    API.onEntityDataChange.connect(entityDataChanged);
    API.onUpdate.connect(onUpdateRenderer);
    API.onUpdate.connect(onUpdateLogic);

    // Call initialisation
    resource.FuelApi.Imaging.Initialize();
    checkPlayerVehicle();
});

function playerEnterVehicle(player: GTANetwork.Util.LocalHandle) {
    checkPlayerVehicle();
}

function playerExitVehicle(player: GTANetwork.Util.LocalHandle) {
    if (currentlyDrivingVehicle != null && currentVehicleFuelStatus != null) {
        API.setEntitySyncedData(currentlyDrivingVehicle, resource.FuelApi.VehicleFuelStatusKey, JSON.stringify(currentVehicleFuelStatus));
    }

    checkPlayerVehicle();
}

function entityDataChanged(entity: GTANetwork.Util.LocalHandle, key: string, oldValue: any) {
    if (currentlyDrivingVehicle == null ||
        currentlyDrivingVehicle.Value != entity.Value ||
        key != resource.FuelApi.VehicleFuelStatusKey) {
        return;
    }
    
    getCurrentVehicleStatus();
}

var lastCheck = new Date().getTime();
var lookingAtGasPumpEntity: GTANetwork.Util.LocalHandle = null;
var lookingAtGasPumpPosition: Vector3 = null;
function onUpdateRenderer() {
    if (currentlyDrivingVehicle != null && currentVehicleFuelStatus != null) {
        resource.FuelApi.Imaging.RenderGauge(currentVehicleFuelStatus);
    }

    if (currentlyDrivingVehicle == null) {
        handleRayTracing();
    }
}

var lastUpdate = new Date().getTime();
function onUpdateLogic() {
    if (currentlyDrivingVehicle == null || currentVehicleFuelStatus == null) {
        return;
    }

    var currentTime = new Date().getTime();
    var updateDelta = currentTime - lastUpdate;
    lastUpdate = currentTime;

    var rpm = API.getVehicleRPM(currentlyDrivingVehicle);
    var consumption = ((rpm * currentVehicleFuelStatus.ConsumptionMultiplier) / 100000) * updateDelta;
    currentVehicleFuelStatus.CurrentFuel = Math.max(0, currentVehicleFuelStatus.CurrentFuel - consumption);

    if (currentVehicleFuelStatus.CurrentFuel <= 0 && !API.getEngineStatus(currentlyDrivingVehicle)) {
        API.setVehicleEngineStatus(currentlyDrivingVehicle, false);
    }
}

function handleRayTracing() {
    // Check if looking at gas pump
    var currentTime = new Date().getTime();
    if (currentTime - lastCheck > 100) {
        lookingAtGasPumpEntity = resource.FuelApi.GetLookingAtGasPumpEntity();
        lookingAtGasPumpPosition = lookingAtGasPumpEntity != null ? API.getEntityPosition(lookingAtGasPumpEntity) : null;
    }

    if (lookingAtGasPumpEntity == null || lookingAtGasPumpPosition == null) {
        return;
    }
    
    resource.FuelApi.Imaging.RenderGasPumpIcon(lookingAtGasPumpPosition);

    API.disableControlThisFrame(resource.FuelApi.UsePumpControl);

    if (API.isDisabledControlJustReleased(resource.FuelApi.UsePumpControl)) {
        API.triggerServerEvent(resource.FuelApi.UsePumpEvent, lookingAtGasPumpPosition, lookingAtGasPumpEntity.Value);
    }
}

function checkPlayerVehicle() {
    // Check if in vehicle
    var currentVehicle = API.getPlayerVehicle(localPlayer);
    if (currentVehicle == null) {
        currentlyDrivingVehicle = null;
        currentVehicleFuelStatus = null;
        return;
    }

    // Check if drivers seat
    var seat = API.getPlayerVehicleSeat(localPlayer);
    if (seat != -1) {
        currentlyDrivingVehicle = null;
        currentVehicleFuelStatus = null;
        return;
    }

    // Check if already current vehicle
    if (currentlyDrivingVehicle != null && currentlyDrivingVehicle.Value == currentVehicle.Value) {
        return;
    }

    // Set current vehicle, and get fuel info
    currentVehicleFuelStatus = null;
    currentlyDrivingVehicle = currentVehicle;
    getCurrentVehicleStatus();
}

function getCurrentVehicleStatus() {
    if (currentlyDrivingVehicle == null) {
        return;
    }

    var fuelStatusJson = API.getEntitySyncedData(currentlyDrivingVehicle, resource.FuelApi.VehicleFuelStatusKey);
    currentVehicleFuelStatus = <VehicleFuelStatus>JSON.parse(fuelStatusJson);

    if (typeof (currentVehicleFuelStatus) == "undefined" || currentVehicleFuelStatus == null) {
        return;
    }

    API.setVehicleEngineStatus(currentlyDrivingVehicle, currentVehicleFuelStatus.CurrentFuel > 0);

    API.sendChatMessage("Fuel status: " + fuelStatusJson);
}