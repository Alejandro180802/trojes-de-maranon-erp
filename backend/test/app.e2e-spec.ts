import { INestApplication } from '@nestjs/common';
import { Test } from '@nestjs/testing';
import request from 'supertest';
import { AppModule } from '../src/app.module';
import { PrismaService } from '../src/prisma/prisma.service';
import { AuthService } from '../src/auth/auth.service';

describe('API security and health (e2e)', () => {
  let app: INestApplication;
  beforeAll(async () => {
    const moduleRef = await Test.createTestingModule({ imports: [AppModule] })
      .overrideProvider(PrismaService).useValue({ $queryRaw: jest.fn().mockResolvedValue([{ '?column?': 1 }]) })
      .overrideProvider(AuthService).useValue({ login: jest.fn().mockResolvedValue({ accessToken: 'access', refreshToken: 'refresh', user: { id: 'user', name: 'Admin', email: 'admin@trojes.local', role: 'ADMIN' } }), refresh: jest.fn(), logout: jest.fn() })
      .compile();
    app = moduleRef.createNestApplication();
    app.setGlobalPrefix('api/v1');
    await app.init();
  });
  afterAll(async () => app.close());

  it('keeps health public', async () => { await request(app.getHttpServer()).get('/api/v1/health').expect(200).expect({ status: 'ok' }); });
  it('keeps login public', async () => { await request(app.getHttpServer()).post('/api/v1/auth/login').send({ email: 'admin@trojes.local', password: 'Admin123!' }).expect(201); });
  it('protects operational analytics', async () => { await request(app.getHttpServer()).get('/api/v1/analytics').expect(401); });
});
