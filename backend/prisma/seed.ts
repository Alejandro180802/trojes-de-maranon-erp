import { PrismaClient, UserRole, WarehouseKind } from '@prisma/client';
import bcrypt from 'bcryptjs';
const prisma = new PrismaClient();
async function main() {
  const passwordHash = await bcrypt.hash(process.env.SEED_ADMIN_PASSWORD ?? 'Admin123!', 12);
  await prisma.user.upsert({ where: { email: 'admin@trojes.local' }, update: {}, create: { email: 'admin@trojes.local', name: 'Administrador', passwordHash, role: UserRole.ADMIN } });
  await prisma.warehouse.upsert({ where: { id: 'seed-fuel-tank' }, update: {}, create: { id: 'seed-fuel-tank', name: 'Tanque principal', kind: WarehouseKind.FUEL_TANK } });
  const defaults: Record<string, UserRole[]> = {
    INVENTORY_CAPTURE: [UserRole.ADMIN, UserRole.SUPERVISOR, UserRole.WAREHOUSE, UserRole.EQUIPMENT], INVENTORY_APPROVE: [UserRole.ADMIN, UserRole.APPROVER], INVENTORY_CANCEL: [UserRole.ADMIN, UserRole.APPROVER, UserRole.SUPERVISOR], REPORT_CAPTURE: [UserRole.ADMIN, UserRole.APPROVER, UserRole.SUPERVISOR, UserRole.WAREHOUSE, UserRole.EQUIPMENT], REPORT_PUBLISH: [UserRole.ADMIN, UserRole.APPROVER, UserRole.SUPERVISOR], REPORT_APPROVE: [UserRole.ADMIN, UserRole.APPROVER], EQUIPMENT_CAPTURE: [UserRole.ADMIN, UserRole.EQUIPMENT, UserRole.SUPERVISOR],
  };
  await Promise.all(Object.entries(defaults).flatMap(([permission, allowedRoles]) => Object.values(UserRole).map((role) => prisma.permissionGrant.upsert({ where: { role_permission: { role, permission } }, update: {}, create: { role, permission, allowed: allowedRoles.includes(role) } }))));
}
main().then(() => prisma.$disconnect()).catch(async (error) => { console.error(error); await prisma.$disconnect(); process.exit(1); });
