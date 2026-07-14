import { Module } from '@nestjs/common';
import { APP_GUARD } from '@nestjs/core';
import { JwtAuthGuard } from '../auth/jwt-auth.guard';
import { RolesGuard } from '../auth/roles.guard';
import { OperationsController } from './operations.controller';
import { OperationsService } from './operations.service';
@Module({ controllers: [OperationsController], providers: [OperationsService, { provide: APP_GUARD, useClass: JwtAuthGuard }, { provide: APP_GUARD, useClass: RolesGuard }] })
export class OperationsModule {}
