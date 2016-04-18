using MinesweeperRobot.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Icons
{
    public class IconScanner
    {
        public IconScanner(params Icon[] icons)
        {
            if (icons == null) throw new ArgumentNullException();
            if (icons.Length <= 1) throw new ArgumentException();

            this.icons = icons;

            var maxWidth = icons.Max(t => t.IntegerMap.Size.Width);
            var maxHeight = icons.Max(t => t.IntegerMap.Size.Height);
            var scannerPoints = EnumerableUtil.Rectangle(maxWidth, maxHeight).Select(point =>
            {
                var validIcons = icons.Where(icon => icon.IntegerMap.Size.Contains(point));

                var pixelLookup = validIcons.ToLookup(t => t.IntegerMap[point.X, point.Y]);
                var scannerValues = pixelLookup.Select(t => new IconScannerValue { Pixel = t.Key, Icons = t.ToArray() });

                return new IconScannerPoint { Point = point, Values = scannerValues.ToArray() };
            }).ToArray();

            this.scannerPoints = ReduceScannerPoints(scannerPoints).ToArray();
        }
        private Icon[] icons;

        public IEnumerable<Tuple<Point, Icon>> QuickScan(IntegerMap targetMap)
        {
            var points = EnumerableUtil.Rectangle(targetMap.Size);
            var reads = points.Select(point =>
            {
                var icon = QuickRead(targetMap, point);
                return Tuple.Create(point, icon);
            });

            return reads.Where(t => t.Item2 != null);
        }
        public IEnumerable<Tuple<Point, Icon>> FullScan(IntegerMap targetMap)
        {
            var quickScan = QuickScan(targetMap);

            return quickScan.Where(t =>
            {
                var fullRead = FullRead(targetMap, t.Item1, new[] { t.Item2 });
                return fullRead == t.Item2;
            });
        }

        private const int scannerPointCount = 10;
        private IconScannerPoint[] scannerPoints;
        private static IEnumerable<IconScannerPoint> ReduceScannerPoints(IEnumerable<IconScannerPoint> scannerPoints)
        {
            var validScannerPoints = new List<IconScannerPoint>();

            var icons = scannerPoints.SelectMany(t => t.Values.SelectMany(s => s.Icons)).Distinct().ToChain();

            var prevConfusions = new[] { icons.ToChain() };
            for (int i = 0; i < scannerPointCount && prevConfusions.Any(); i++)
            {
                var remainingScannerPoints = scannerPoints.Except(validScannerPoints);
                if (remainingScannerPoints.Any() == false) break;

                var scannerPointConfusions = remainingScannerPoints.ToDictionary(t => t, scannerPoint =>
                {
                    var confusions = prevConfusions.SelectMany(confusedGroup =>
                    {
                        return scannerPoint.Values.SelectMany(scannerPointValue =>
                        {
                            return new[]
                            {
                                confusedGroup.Intersect(scannerPointValue.Icons).ToChain(),
                                confusedGroup.Except(scannerPointValue.Icons).ToChain()
                            };
                        });
                    }).Distinct().ToArray();

                    var concludeds = confusions.Where(t => t.Count() == 1).Select(t => t.Single()).ToArray();
                    return confusions.Where(t => t.Count() >= 2).Select(t => t.Except(concludeds).ToChain()).ToArray();
                });

                var orderedScannerPointConfusions = scannerPointConfusions.OrderBy(scannerPointConfusion =>
                    scannerPointConfusion.Value.Any() ? scannerPointConfusion.Value.Max(t => t.Count()) : 0);

                var nextScannerPoint = orderedScannerPointConfusions.First();
                validScannerPoints.Add(nextScannerPoint.Key);

                prevConfusions = nextScannerPoint.Value;
            }

            return validScannerPoints;
        }

        public Icon QuickRead(IntegerMap targetMap, Point offset)
        {
            var validIcons = icons.Where(icon => targetMap.Size.Contains(offset, icon.IntegerMap.Size)).ToArray();
            var validPoints = scannerPoints.Where(point => targetMap.Size.Contains(point.Point.Add(offset)));

            foreach (var scannerPoint in validPoints)
            {
                var point = scannerPoint.Point.Add(offset);
                var pixel = targetMap[point.X, point.Y];

                var scannerValue = scannerPoint.Values.FirstOrDefault(t => t.Pixel == pixel);
                if (scannerValue.EqualsDefault()) return null;

                validIcons = validIcons.Intersect(scannerValue.Icons).ToArray();

                if (validIcons.Length <= 0) return null;
                if (validIcons.Length == 1)
                {
                    return validIcons.Single();
                }
            }

            return FullRead(targetMap, offset, validIcons);
        }
        public Icon FullRead(IntegerMap targetMap, Point offset, IEnumerable<Icon> candidateIcons)
        {
            var validIcons = candidateIcons.Where(icon => targetMap.Size.Contains(offset, icon.IntegerMap.Size));
            return validIcons.FirstOrDefault(icon =>
            {
                var iconPoints = EnumerableUtil.Rectangle(icon.IntegerMap.Size);
                return iconPoints.All(iconPoint =>
                {
                    var targetPoint = iconPoint.Add(offset);
                    return targetMap[targetPoint.X, targetPoint.Y] == icon.IntegerMap[iconPoint.X, iconPoint.Y];
                });
            });
        }
    }
}
