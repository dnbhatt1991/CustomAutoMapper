namespace Mapper
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Reflection;

    public class Mapper
    {
        private object MapObject(object source, object dest)
        {
            var destProperties = dest.GetType()
                                     .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                     .Where(x => x.CanWrite);

            foreach (var destProp in destProperties)
            {
                var sourceProp = source
                                       .GetType()
                                       .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                       .FirstOrDefault(x => x.Name == destProp.Name);

                if (sourceProp != null)
                {
                    var sourceValue = sourceProp.GetMethod.Invoke(source, null);

                    if (sourceValue == null)
                    {
                        throw new ArgumentNullException($"{sourceProp.Name} is null!");
                    }


                    if (ReflectionUtils.IsPrimitive(sourceValue.GetType()))
                    {
                        destProp.SetValue(dest, sourceProp.GetValue(source, null));

                        continue;
                    }

                    if (ReflectionUtils.IsGenericCollection(sourceValue.GetType()))
                    {
                        if (ReflectionUtils.IsPrimitive(sourceValue.GetType().GetGenericArguments()[0]))
                        {
                            var destinationCollection = sourceValue;
                            destProp.SetMethod.Invoke(dest, new[] {destinationCollection});
                        }
                        else
                        {
                            var destColl = Activator.CreateInstance(sourceValue.GetType());
                            var destType = destColl.GetType().GetGenericArguments()[0];

                            foreach (var destP in (IEnumerable)sourceValue)
                            {
                                ((IList) destColl).Add(this.CreateMapperObject(destP, destType));
                            }

                            destProp.SetMethod.Invoke(dest, new []{destColl});
                        }
                    }
                    else if (ReflectionUtils.IsNonGenericCollection(sourceValue.GetType()))
                    {
                        var destColl = (IList) Activator
                            .CreateInstance(destProp.PropertyType, new object[] {((object[]) sourceValue).Length});

                        for (int i = 0; i < ((object[])sourceValue).Length; i++)
                        {
                            destColl[i] = this.CreateMapperObject(((object[]) sourceValue)[i],
                                destProp.PropertyType.GetElementType());
                        }

                        destProp.SetValue(dest, destColl);
                    }
                    else
                    {
                        destProp.SetValue(dest,
                            this.CreateMapperObject(sourceProp.GetValue(source), destProp.PropertyType));
                    }
                }
            }

            return dest;
        }

        public object CreateMapperObject(object source, Type destType)
        {
            if (source == null)
            {
                throw new ArgumentNullException($"Source object cannot be null!");
            }

            if (destType == null)
            {
                throw new ArgumentNullException($"DestType cannot be null!");
            }

            var dest = Activator.CreateInstance(destType);

            return this.MapObject(source, dest);
        }

        public TDest CreateMapperObject<TDest>(object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException($"Source object cannot be null!");
            }

            var dest = Activator.CreateInstance(typeof(TDest));

            return (TDest) this.MapObject(source, dest);
        }
    }
}