import { Body, Controller, Get, Header, Param, Patch, Post, Query, Req } from '@nestjs/common';
import { UserRole } from '@prisma/client';
import { Roles } from '../auth/roles.decorator';
import { ActivityDto, AssignmentDto, CreateUserDto, EstimateDto, FuelDto, MachineDto, MachineStatusDto, MaintenanceDto, MaintenanceRecordDto, MaterialDto, MovementDto, PermissionGrantDto, PlatformDto, ProjectDto, RepairDto, ReportDto, SetActiveDto, UpdateActivityDto, UpdateUserDto, WarehouseDto } from './dto';
import { OperationsService } from './operations.service';
type RequestUser = { user: { id: string; role: UserRole } };
@Controller() export class OperationsController {
  constructor(private readonly service: OperationsService) {}
  @Get('dashboard') dashboard() { return this.service.dashboard(); }
  @Get('analytics') analytics() { return this.service.analytics(); }
  @Get('exports/inventory.csv') @Header('Content-Type', 'text/csv; charset=utf-8') @Header('Content-Disposition', 'attachment; filename="inventario.csv"') inventoryCsv() { return this.service.inventoryCsv(); }
  @Get('exports/reports.csv') @Header('Content-Type', 'text/csv; charset=utf-8') @Header('Content-Disposition', 'attachment; filename="reportes-diarios.csv"') reportsCsv() { return this.service.reportsCsv(); }
  @Get('exports/movements.csv') @Header('Content-Type', 'text/csv; charset=utf-8') @Header('Content-Disposition', 'attachment; filename="movimientos.csv"') movementsCsv() { return this.service.movementsCsv(); }
  @Get('exports/fuel.csv') @Header('Content-Type', 'text/csv; charset=utf-8') @Header('Content-Disposition', 'attachment; filename="diesel.csv"') fuelCsv() { return this.service.fuelCsv(); }
  @Get('exports/equipment.csv') @Header('Content-Type', 'text/csv; charset=utf-8') @Header('Content-Disposition', 'attachment; filename="maquinaria.csv"') equipmentCsv() { return this.service.equipmentCsv(); }
  @Get('exports/comparisons.csv') @Header('Content-Type', 'text/csv; charset=utf-8') @Header('Content-Disposition', 'attachment; filename="estimado-real.csv"') comparisonsCsv() { return this.service.comparisonsCsv(); }
  @Get('materials') materials(@Query('includeInactive') includeInactive?: string) { return this.service.materials(includeInactive === 'true'); }
  @Post('materials') @Roles(UserRole.ADMIN, UserRole.WAREHOUSE) material(@Body() dto: MaterialDto, @Req() req: RequestUser) { return this.service.createMaterial(dto, req.user); }
  @Patch('materials/:id/active') @Roles(UserRole.ADMIN, UserRole.WAREHOUSE) materialActive(@Param('id') id: string, @Body() dto: SetActiveDto, @Req() req: RequestUser) { return this.service.setMaterialActive(id, dto.active, req.user); }
  @Get('warehouses') warehouses(@Query('includeInactive') includeInactive?: string) { return this.service.warehouses(includeInactive === 'true'); }
  @Post('warehouses') @Roles(UserRole.ADMIN, UserRole.WAREHOUSE) warehouse(@Body() dto: WarehouseDto, @Req() req: RequestUser) { return this.service.createWarehouse(dto, req.user); }
  @Patch('warehouses/:id/active') @Roles(UserRole.ADMIN, UserRole.WAREHOUSE) warehouseActive(@Param('id') id: string, @Body() dto: SetActiveDto, @Req() req: RequestUser) { return this.service.setWarehouseActive(id, dto.active, req.user); }
  @Get('users') @Roles(UserRole.ADMIN) users() { return this.service.users(); }
  @Post('users') @Roles(UserRole.ADMIN) user(@Body() dto: CreateUserDto, @Req() req: RequestUser) { return this.service.createUser(dto, req.user); }
  @Patch('users/:id') @Roles(UserRole.ADMIN) updateUser(@Param('id') id: string, @Body() dto: UpdateUserDto, @Req() req: RequestUser) { return this.service.updateUser(id, dto, req.user); }
  @Get('projects') projects() { return this.service.projects(); }
  @Post('projects') @Roles(UserRole.ADMIN, UserRole.APPROVER) project(@Body() dto: ProjectDto, @Req() req: RequestUser) { return this.service.createProject(dto, req.user); }
  @Post('platforms') @Roles(UserRole.ADMIN, UserRole.APPROVER) platform(@Body() dto: PlatformDto, @Req() req: RequestUser) { return this.service.createPlatform(dto, req.user); }
  @Get('platforms') platforms() { return this.service.platforms(); }
  @Get('platforms/:id/activities') activities(@Param('id') id: string) { return this.service.activities(id); }
  @Get('platforms/:id/summary') platformSummary(@Param('id') id: string) { return this.service.platformSummary(id); }
  @Post('activities') activity(@Body() dto: ActivityDto, @Req() req: RequestUser) { return this.service.createActivity(dto, req.user); }
  @Patch('activities/:id') activityUpdate(@Param('id') id: string, @Body() dto: UpdateActivityDto, @Req() req: RequestUser) { return this.service.updateActivity(id, dto, req.user); }
  @Post('estimates') @Roles(UserRole.ADMIN, UserRole.APPROVER) estimate(@Body() dto: EstimateDto, @Req() req: RequestUser) { return this.service.upsertEstimate(dto, req.user); }
  @Get('estimates') estimates() { return this.service.estimates(); }
  @Get('estimates/comparison') comparison() { return this.service.consumptionComparison(); }
  @Get('inventory/balances') balances() { return this.service.balances(); }
  @Get('inventory/movements') movements() { return this.service.movements(); }
  @Post('inventory/movements') movement(@Body() dto: MovementDto, @Req() req: RequestUser) { return this.service.createMovement(dto, req.user); }
  @Post('inventory/movements/:id/publish') publishMovement(@Param('id') id: string, @Req() req: RequestUser) { return this.service.publishMovement(id, req.user); }
  @Post('inventory/movements/:id/cancel') cancelMovement(@Param('id') id: string, @Req() req: RequestUser) { return this.service.cancelMovement(id, req.user); }
  @Get('machines') machines() { return this.service.machines(); }
  @Post('machines') machine(@Body() dto: MachineDto, @Req() req: RequestUser) { return this.service.createMachine(dto, req.user); }
  @Patch('machines/:id/status') machineStatus(@Param('id') id: string, @Body() dto: MachineStatusDto, @Req() req: RequestUser) { return this.service.setMachineStatus(id, dto.status, req.user); }
  @Post('machine-assignments') assignment(@Body() dto: AssignmentDto, @Req() req: RequestUser) { return this.service.assignMachine(dto, req.user); }
  @Get('machine-assignments') assignments() { return this.service.assignments(); }
  @Post('fuel-logs') fuel(@Body() dto: FuelDto, @Req() req: RequestUser) { return this.service.fuel(dto, req.user); }
  @Get('fuel-logs') fuelLogs() { return this.service.fuelLogs(); }
  @Post('maintenance-plans') maintenance(@Body() dto: MaintenanceDto, @Req() req: RequestUser) { return this.service.createMaintenance(dto, req.user); }
  @Get('maintenance-records') maintenanceRecords() { return this.service.maintenanceRecords(); }
  @Post('maintenance-records') maintenanceRecord(@Body() dto: MaintenanceRecordDto, @Req() req: RequestUser) { return this.service.recordMaintenance(dto, req.user); }
  @Get('repairs') repairs() { return this.service.repairs(); }
  @Post('repairs') repair(@Body() dto: RepairDto, @Req() req: RequestUser) { return this.service.createRepair(dto, req.user); }
  @Get('daily-reports') reports() { return this.service.reports(); }
  @Post('daily-reports') report(@Body() dto: ReportDto, @Req() req: RequestUser) { return this.service.createReport(dto, req.user); }
  @Post('daily-reports/:id/publish') publishReport(@Param('id') id: string, @Req() req: RequestUser) { return this.service.publishReport(id, req.user); }
  @Post('daily-reports/:id/approve') approveReport(@Param('id') id: string, @Req() req: RequestUser) { return this.service.approveReport(id, req.user); }
  @Get('alerts') alerts() { return this.service.alerts(); }
  @Post('alerts/:id/resolve') resolveAlert(@Param('id') id: string, @Req() req: RequestUser) { return this.service.resolveAlert(id, req.user); }
  @Get('permissions') @Roles(UserRole.ADMIN) permissions() { return this.service.permissions(); }
  @Patch('permissions') @Roles(UserRole.ADMIN) permission(@Body() dto: PermissionGrantDto, @Req() req: RequestUser) { return this.service.setPermission(dto, req.user); }
}
