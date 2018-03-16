       List<double> Mag(IEnumerable<ComplexD> list)
        {
            var result = new List<double>();
            foreach (var v in list)
                result.Add(Mag(v));
            return result;
        }
        List<double> Hamming(IEnumerable<double> list)
        {
            var max = list.Max(x => Math.Abs(x));

            var result = new List<double>();
            foreach (var v in list)
            {
                var s = hammingWindow[(int)(v / max * (windowLen - 1))];
                result.Add(v * s);

            }

            return result;
        }

        List<double> Hamming(IEnumerable<ComplexD> list)
        {
            var max = list.Max(v => Math.Sqrt(v.x * v.x + v.y * v.y));

            var result = new List<double>();
            foreach (var v in list)
            {
                var mag = Math.Sqrt(v.x * v.x + v.y * v.y);

                var s = hammingWindow[(int)(mag / max * (windowLen - 1))];
                result.Add(mag * s);

            }

            return result;
        }
