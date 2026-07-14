import { Injectable, UnauthorizedException } from '@nestjs/common';
import { JwtService } from '@nestjs/jwt';
import { User } from '@prisma/client';
import bcrypt from 'bcryptjs';
import { createHash, randomUUID } from 'crypto';
import { PrismaService } from '../prisma/prisma.service';
@Injectable() export class AuthService {
  constructor(private readonly prisma: PrismaService, private readonly jwt: JwtService) {}
  async login(email: string, password: string) {
    const user = await this.prisma.user.findUnique({ where: { email } });
    if (!user || !user.active || !(await bcrypt.compare(password, user.passwordHash))) throw new UnauthorizedException('Credenciales inválidas');
    return this.createSession(user);
  }
  async refresh(token: string) {
    const record = await this.prisma.refreshToken.findUnique({ where: { tokenHash: await this.hash(token) }, include: { user: true } });
    if (!record || record.revokedAt || record.expiresAt <= new Date() || !record.user.active) throw new UnauthorizedException('Sesión expirada');
    await this.prisma.refreshToken.update({ where: { id: record.id }, data: { revokedAt: new Date() } });
    return this.createSession(record.user);
  }
  async logout(token: string) { await this.prisma.refreshToken.updateMany({ where: { tokenHash: await this.hash(token), revokedAt: null }, data: { revokedAt: new Date() } }); }
  async createSession(user: Pick<User, 'id' | 'email' | 'name' | 'role'>) {
    const accessToken = await this.jwt.signAsync({ sub: user.id, email: user.email, role: user.role });
    const refreshToken = randomUUID() + randomUUID();
    const expiresAt = new Date(Date.now() + Number(process.env.REFRESH_TOKEN_DAYS ?? 30) * 86400000);
    await this.prisma.refreshToken.create({ data: { tokenHash: await this.hash(refreshToken), userId: user.id, expiresAt } });
    return { accessToken, refreshToken, user: { id: user.id, email: user.email, name: user.name, role: user.role } };
  }
  private async hash(value: string) { return createHash('sha256').update(value).digest('hex'); }
}
