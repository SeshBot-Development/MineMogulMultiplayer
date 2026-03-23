using System;

// Token: 0x020000B4 RID: 180
public enum SavableObjectID
{
	// Token: 0x04000587 RID: 1415
	INVALID,
	// Token: 0x04000588 RID: 1416
	CannonTarget = 100,
	// Token: 0x04000589 RID: 1417
	Conveyor90,
	// Token: 0x0400058A RID: 1418
	Conveyor90_mirror,
	// Token: 0x0400058B RID: 1419
	ConveyorBlocker,
	// Token: 0x0400058C RID: 1420
	ConveyorCannon,
	// Token: 0x0400058D RID: 1421
	ConveyorDown,
	// Token: 0x0400058E RID: 1422
	ConveyorSorter,
	// Token: 0x0400058F RID: 1423
	ConveyorSorter_Mirror,
	// Token: 0x04000590 RID: 1424
	ConveyorStraight,
	// Token: 0x04000591 RID: 1425
	ConveyorTCombiner,
	// Token: 0x04000592 RID: 1426
	ConveyorTSplitter,
	// Token: 0x04000593 RID: 1427
	ConveyorUp,
	// Token: 0x04000594 RID: 1428
	LCombiner,
	// Token: 0x04000595 RID: 1429
	LCombiner_Mirror,
	// Token: 0x04000596 RID: 1430
	RollerStraight,
	// Token: 0x04000597 RID: 1431
	RollerUp,
	// Token: 0x04000598 RID: 1432
	RobotGrabberArm,
	// Token: 0x04000599 RID: 1433
	ConveyorLedge,
	// Token: 0x0400059A RID: 1434
	RollerConveyor90,
	// Token: 0x0400059B RID: 1435
	RollerConveyor90_Mirror,
	// Token: 0x0400059C RID: 1436
	ConveyorRouting_Left,
	// Token: 0x0400059D RID: 1437
	ConveyorRouting_Right,
	// Token: 0x0400059E RID: 1438
	RollerConveyorRouting_Left,
	// Token: 0x0400059F RID: 1439
	RollerConveyorRouting_Right,
	// Token: 0x040005A0 RID: 1440
	ConveyorBlockerT2,
	// Token: 0x040005A1 RID: 1441
	ConveyorSplitterT2,
	// Token: 0x040005A2 RID: 1442
	Sorter_Lid,
	// Token: 0x040005A3 RID: 1443
	WallConveyor_Straight,
	// Token: 0x040005A4 RID: 1444
	WallConveyor_Up,
	// Token: 0x040005A5 RID: 1445
	WallConveyor_Down,
	// Token: 0x040005A6 RID: 1446
	WallConveyor_Ledge,
	// Token: 0x040005A7 RID: 1447
	WallConveyor_90,
	// Token: 0x040005A8 RID: 1448
	WallConveyor_90_Mirror,
	// Token: 0x040005A9 RID: 1449
	OreOverflowSplitter,
	// Token: 0x040005AA RID: 1450
	RollerConveyorUp,
	// Token: 0x040005AB RID: 1451
	RollerConveyorDown,
	// Token: 0x040005AC RID: 1452
	VerticalBulkSorter,
	// Token: 0x040005AD RID: 1453
	WallConveyor_Straight_LeftOnly,
	// Token: 0x040005AE RID: 1454
	WallConveyor_Straight_RightOnly,
	// Token: 0x040005AF RID: 1455
	RollerSorter,
	// Token: 0x040005B0 RID: 1456
	RollerSorter_Mirror,
	// Token: 0x040005B1 RID: 1457
	Ingot_StraighteningConveyor,
	// Token: 0x040005B2 RID: 1458
	ConveyorCombiner3To1,
	// Token: 0x040005B3 RID: 1459
	Wall_LCombiner,
	// Token: 0x040005B4 RID: 1460
	Wall_LCombiner_Mirror,
	// Token: 0x040005B5 RID: 1461
	Wall_TCombiner,
	// Token: 0x040005B6 RID: 1462
	ConveyorSplitterT2_Left,
	// Token: 0x040005B7 RID: 1463
	ConveyorSplitterT2_Right,
	// Token: 0x040005B8 RID: 1464
	RollerSplitter_Left,
	// Token: 0x040005B9 RID: 1465
	RollerSplitter_Right,
	// Token: 0x040005BA RID: 1466
	AutoMinerMk1 = 201,
	// Token: 0x040005BB RID: 1467
	BlastFurnace,
	// Token: 0x040005BC RID: 1468
	Grinder,
	// Token: 0x040005BD RID: 1469
	Hopper,
	// Token: 0x040005BE RID: 1470
	OreAnalyzer,
	// Token: 0x040005BF RID: 1471
	OreAnalyzer_Mirror,
	// Token: 0x040005C0 RID: 1472
	PolishingMachine,
	// Token: 0x040005C1 RID: 1473
	RollingMill,
	// Token: 0x040005C2 RID: 1474
	PipeRoller,
	// Token: 0x040005C3 RID: 1475
	Packager,
	// Token: 0x040005C4 RID: 1476
	RodExtruder,
	// Token: 0x040005C5 RID: 1477
	RapidAutoMiner,
	// Token: 0x040005C6 RID: 1478
	ShakerTable,
	// Token: 0x040005C7 RID: 1479
	CastingFurnace,
	// Token: 0x040005C8 RID: 1480
	ThreadingLathe,
	// Token: 0x040005C9 RID: 1481
	HeavyAutoMiner,
	// Token: 0x040005CA RID: 1482
	HeavyCrusher,
	// Token: 0x040005CB RID: 1483
	PipeRoller_Mirror,
	// Token: 0x040005CC RID: 1484
	LampPole = 301,
	// Token: 0x040005CD RID: 1485
	WoodPlatform2x2,
	// Token: 0x040005CE RID: 1486
	WoodPlatform_Ramp2x2,
	// Token: 0x040005CF RID: 1487
	Chest1x2x1,
	// Token: 0x040005D0 RID: 1488
	Arrow_Sign,
	// Token: 0x040005D1 RID: 1489
	Arrow_Sign_Mirror,
	// Token: 0x040005D2 RID: 1490
	Wood_Door_1x,
	// Token: 0x040005D3 RID: 1491
	Wood_Door_2x,
	// Token: 0x040005D4 RID: 1492
	Wood_Roof_1x2,
	// Token: 0x040005D5 RID: 1493
	Wood_Roof_2x2,
	// Token: 0x040005D6 RID: 1494
	Wood_ShortWall_1x,
	// Token: 0x040005D7 RID: 1495
	Wood_ShortWall_2x,
	// Token: 0x040005D8 RID: 1496
	Wood_SideRoof_2x,
	// Token: 0x040005D9 RID: 1497
	Wood_SideRoof_2x_Mirror,
	// Token: 0x040005DA RID: 1498
	Wood_Wall_1x,
	// Token: 0x040005DB RID: 1499
	Wood_Wall_2x,
	// Token: 0x040005DC RID: 1500
	Wood_Window_1x,
	// Token: 0x040005DD RID: 1501
	Wood_Stairs1x1,
	// Token: 0x040005DE RID: 1502
	Wood_Stairs1x2,
	// Token: 0x040005DF RID: 1503
	WoodPlatform1x2,
	// Token: 0x040005E0 RID: 1504
	WoodPlatform1x1,
	// Token: 0x040005E1 RID: 1505
	WoodPlatform2x2_Thin,
	// Token: 0x040005E2 RID: 1506
	WoodPlatform_Ramp2x2_Thin,
	// Token: 0x040005E3 RID: 1507
	WoodPlatform1x2_Thin,
	// Token: 0x040005E4 RID: 1508
	WoodPlatform1x1_Thin,
	// Token: 0x040005E5 RID: 1509
	Metal_Roof_2x2,
	// Token: 0x040005E6 RID: 1510
	Metal_Roof_1x2,
	// Token: 0x040005E7 RID: 1511
	ToolBuilder = 401,
	// Token: 0x040005E8 RID: 1512
	HammerBasic,
	// Token: 0x040005E9 RID: 1513
	Lantern,
	// Token: 0x040005EA RID: 1514
	MagnetTool,
	// Token: 0x040005EB RID: 1515
	PickaxeBasic,
	// Token: 0x040005EC RID: 1516
	ResourceScannerTool,
	// Token: 0x040005ED RID: 1517
	RapidAutoMinerStandardDrillBit,
	// Token: 0x040005EE RID: 1518
	RapidAutoMinerTurboDrillBit,
	// Token: 0x040005EF RID: 1519
	RapidAutoMinerHardenedDrillBit,
	// Token: 0x040005F0 RID: 1520
	WrenchTool,
	// Token: 0x040005F1 RID: 1521
	DebugSpawnTool,
	// Token: 0x040005F2 RID: 1522
	IngotMold,
	// Token: 0x040005F3 RID: 1523
	GearMold,
	// Token: 0x040005F4 RID: 1524
	DoubleIngotMold,
	// Token: 0x040005F5 RID: 1525
	JackHammer,
	// Token: 0x040005F6 RID: 1526
	HardHat,
	// Token: 0x040005F7 RID: 1527
	MiningHelmet,
	// Token: 0x040005F8 RID: 1528
	BuildingCrate1x1x1 = 501,
	// Token: 0x040005F9 RID: 1529
	BuildingCrate1x1x2,
	// Token: 0x040005FA RID: 1530
	BuildingCrate1x2x1,
	// Token: 0x040005FB RID: 1531
	BuildingCrateAutoMiner,
	// Token: 0x040005FC RID: 1532
	BuildingCrate1x2x2,
	// Token: 0x040005FD RID: 1533
	Chute = 600,
	// Token: 0x040005FE RID: 1534
	Chute_Window,
	// Token: 0x040005FF RID: 1535
	Chute_Top,
	// Token: 0x04000600 RID: 1536
	Chute_Angled,
	// Token: 0x04000601 RID: 1537
	Chute_Splitter,
	// Token: 0x04000602 RID: 1538
	Chute_Bottom,
	// Token: 0x04000603 RID: 1539
	Chute_Hopper,
	// Token: 0x04000604 RID: 1540
	Chute_Top_Angle,
	// Token: 0x04000605 RID: 1541
	Chute_Top_Angle_Window,
	// Token: 0x04000606 RID: 1542
	Chute_Hatch,
	// Token: 0x04000607 RID: 1543
	Box1x1x1 = 700,
	// Token: 0x04000608 RID: 1544
	HalloweenPumpkin = 800,
	// Token: 0x04000609 RID: 1545
	DiamondHalloweenPumpkin,
	// Token: 0x0400060A RID: 1546
	HolidayLampPole1,
	// Token: 0x0400060B RID: 1547
	HolidayLampPole2,
	// Token: 0x0400060C RID: 1548
	ChristmasTree,
	// Token: 0x0400060D RID: 1549
	Trophy_CoalMogul = 900,
	// Token: 0x0400060E RID: 1550
	Trophy_CopperMogul = 902,
	// Token: 0x0400060F RID: 1551
	Trophy_GoldMogul = 904,
	// Token: 0x04000610 RID: 1552
	Trophy_IronMogul = 906,
	// Token: 0x04000611 RID: 1553
	Trophy_MineMogul = 908,
	// Token: 0x04000612 RID: 1554
	Trophy_MineMogul_Broken,
	// Token: 0x04000613 RID: 1555
	Trophy_SteelMogul,
	// Token: 0x04000614 RID: 1556
	Upgrade_DepositBoxT2 = 1000,
	// Token: 0x04000615 RID: 1557
	DevTestShopItems,
	// Token: 0x04000616 RID: 1558
	BreakableCrateSmall = 1100,
	// Token: 0x04000617 RID: 1559
	BreakableCrateSmall_Empty,
	// Token: 0x04000618 RID: 1560
	BreakableCrateMedium,
	// Token: 0x04000619 RID: 1561
	BreakableCrateTall = 1104,
	// Token: 0x0400061A RID: 1562
	DetonatorBoombox_Physics = 1106
}
