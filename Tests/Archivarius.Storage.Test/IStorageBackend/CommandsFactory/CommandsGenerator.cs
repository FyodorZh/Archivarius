using System;
using System.Collections.Generic;

namespace Archivarius.Storage.Test.StorageBackend
{
    public static class CommandsGenerator
    {
        public static IEnumerable<ITestCommand<IStorageBackend>> Generate(int count, int? rndSeed = null)
        {
            Random rnd = rndSeed != null ? new Random(rndSeed.Value) : new Random();

            int dirDepth = 5;
            int dirNameVariance = 2;
            int fileNameVariance = 4;
            
            for (int i = 0; i < count; ++i)
            {
                switch (rnd.Next(5))
                {
                    case 0:
                        yield return new Read_Command(GetRndFilePath(rnd, GenRndDirPath(rnd, dirDepth, dirNameVariance), fileNameVariance));
                        break;
                    case 1:
                        yield return new IsExists_Command(GetRndFilePath(rnd, GenRndDirPath(rnd, dirDepth, dirNameVariance), fileNameVariance));
                        break;
                    case 2:
                        yield return new GetSubPaths_Command(GenRndDirPath(rnd, dirDepth, dirNameVariance));
                        break;
                    case 3:
                        yield return new Erase_Command(GetRndFilePath(rnd, GenRndDirPath(rnd, dirDepth, dirNameVariance), fileNameVariance));
                        break;
                    case 4:
                        yield return new Write_Command(GetRndFilePath(rnd, GenRndDirPath(rnd, dirDepth, dirNameVariance), fileNameVariance), 
                            GenPayload(rnd, 1024));
                        break;
                }    
            }
        }

        private static FilePath GetRndFilePath(Random rnd, DirPath dirPath, int levelOfVariation = 3)
        {
            return dirPath.File(((char)('a' + rnd.Next(levelOfVariation))).ToString());
        }

        private static DirPath GenRndDirPath(Random rnd, int maxDepth, int levelOfVariation = 2)
        {
            int dirLength = rnd.Next(maxDepth);
            DirPath dir = DirPath.Root;
            for (int i = 0; i < dirLength; ++i)
            {
                dir = dir.Dir(((char)('a' + rnd.Next(levelOfVariation))).ToString());
            }

            return dir;
        }

        private static byte[] GenPayload(Random rnd, int rndMaxSize)
        {
            byte[] bytes = new byte[rnd.Next(rndMaxSize) + 1];
            rnd.NextBytes(bytes);
            return bytes;
        }
    }
}