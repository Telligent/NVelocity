// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace NVelocity.Runtime.Resource
{
	using Commons.Collections;
	using Loader;
	using NVelocity.Exception;
	using System;
	using System.Collections;
				using System.Collections.Generic;
				using System.IO;

	/// <summary> 
	/// Class to manage the text resource for the Velocity Runtime.
	/// </summary>
	public class ResourceManagerImpl : IResourceManager
	{
		public ResourceManagerImpl()
		{
			resourceLoaders = new();
			sourceInitializerList = new();
		}

		/// <summary>
		/// token used to identify the loader internally
		/// </summary>
		private const string RESOURCE_LOADER_IDENTIFIER = "_RESOURCE_LOADER_IDENTIFIER_";

		/// <summary>
		/// object implementing ResourceCache to
		/// be our resource manager's Resource cache.
		/// </summary>
		protected internal ResourceCache globalCache = null;

		/// <summary> 
		/// The List of templateLoaders that the Runtime will
		/// use to locate the InputStream source of a template.
		/// </summary>
		protected internal List<ResourceLoader> resourceLoaders;

		/// <summary>
		/// This is a list of the template input stream source
		/// initializers, basically properties for a particular
		/// template stream source. The order in this list
		/// reflects numbering of the properties i.e.
		/// &lt;loader-id&gt;.resource.loader.&lt;property&gt; = &lt;value&gt;
		/// </summary>
		private readonly List<ExtendedProperties> sourceInitializerList;

		/// <summary>
		/// Each loader needs a configuration object for
		/// its initialization, this flags keeps track of whether
		/// or not the configuration objects have been created
		/// for the resource loaders.
		/// </summary>
		private bool resourceLoaderInitializersActive = false;

		/// <summary>
		/// switch to turn off log notice when a resource is found for
		/// the first time.
		/// </summary>
		private bool logWhenFound = true;

		protected internal IRuntimeServices runtimeServices = null;

		/// <summary>
		/// Initialize the ResourceManager.
		/// </summary>
		public void Initialize(IRuntimeServices rs)
		{
			runtimeServices = rs;

			runtimeServices.Info(string.Format("Default ResourceManager initializing. ({0})", GetType()));

			ResourceLoader resourceLoader;

			AssembleResourceLoaderInitializers();

			for (int i = 0; i < sourceInitializerList.Count; i++)
			{
				ExtendedProperties configuration = sourceInitializerList[i];
				string loaderClass = configuration.GetString("class");

				if (loaderClass == null)
				{
					runtimeServices.Error(
						string.Format(
							"Unable to find '{0}.resource.loader.class' specification in configuration. This is a critical value.  Please adjust configuration.",
							configuration.GetString(RESOURCE_LOADER_IDENTIFIER)));
					continue;
				}

				resourceLoader = ResourceLoaderFactory.getLoader(runtimeServices, loaderClass);
				resourceLoader.CommonInit(runtimeServices, configuration);
				resourceLoader.Init(configuration);
				resourceLoaders.Add(resourceLoader);
			}

			// now see if this is overridden by configuration
			logWhenFound = runtimeServices.GetBoolean(RuntimeConstants.RESOURCE_MANAGER_LOGWHENFOUND, true);

			// now, is a global cache specified?
			string resourceManagerCacheClassName = runtimeServices.GetString(RuntimeConstants.RESOURCE_MANAGER_CACHE_CLASS);
			object o = null;

			if (resourceManagerCacheClassName != null && resourceManagerCacheClassName.Length > 0)
			{
				try
				{
					Type type = Type.GetType(resourceManagerCacheClassName);
					o = Activator.CreateInstance(type);
				}
				catch (Exception)
				{
					string err =
						string.Format(
							"The specified class for ResourceCache ({0}) does not exist (or is not accessible to the current classLoader).",
							resourceManagerCacheClassName);
					runtimeServices.Error(err);
					o = null;
				}

				if (o is not ResourceCache)
				{
					string err =
						string.Format(
							"The specified class for ResourceCache ({0}) does not implement NVelocity.Runtime.Resource.ResourceCache. Using default ResourceCache implementation.",
							resourceManagerCacheClassName);
					runtimeServices.Error(err);
					o = null;
				}
			}

			// if we didn't get through that, just use the default.
			o ??= new ResourceCacheImpl();

			globalCache = (ResourceCache)o;
			globalCache.initialize(runtimeServices);
			runtimeServices.Info("Default ResourceManager initialization complete.");
		}

		private void AssembleResourceLoaderInitializers()
		{
			if (resourceLoaderInitializersActive)
			{
				return;
			}

			var resourceLoaderNames = runtimeServices.Configuration.GetStringList(RuntimeConstants.RESOURCE_LOADER);

			for (int i = 0; i < resourceLoaderNames.Count; i++)
			{
				/*
					* The loader id might look something like the following:
					*
					* file.resource.loader
					*
					* The loader id is the prefix used for all properties
					* pertaining to a particular loader.
					*/
				string loaderID = string.Format("{0}.{1}", resourceLoaderNames[i], RuntimeConstants.RESOURCE_LOADER);

				ExtendedProperties loaderConfiguration = runtimeServices.Configuration.Subset(loaderID);

				/*
					*  we can't really count on ExtendedProperties to give us an empty set
					*/
				if (loaderConfiguration == null)
				{
					runtimeServices.Warn(
						string.Format("ResourceManager : No configuration information for resource loader named '{0}'. Skipping.",
													resourceLoaderNames[i]));
					continue;
				}

				/*
					*  add the loader name token to the initializer if we need it
					*  for reference later. We can't count on the user to fill
					*  in the 'name' field
					*/
				loaderConfiguration.SetProperty(RESOURCE_LOADER_IDENTIFIER, resourceLoaderNames[i]);

				/*
					* Add resources to the list of resource loader
					* initializers.
					*/
				sourceInitializerList.Add(loaderConfiguration);
			}

			resourceLoaderInitializersActive = true;
		}

		/// <summary> Gets the named resource.  Returned class type corresponds to specified type
		/// (i.e. <code>Template</code> to <code>Template</code>).
		/// *
		/// </summary>
		/// <param name="resourceName">The name of the resource to retrieve.
		/// </param>
		/// <param name="resourceType">The type of resource (<code>Template</code>,
		/// <code>Content</code>, etc.).
		/// </param>
		/// <param name="encoding"> The character encoding to use.
		/// </param>
		/// <returns>Resource with the template parsed and ready.
		/// @throws ResourceNotFoundException if template not found
		/// from any available source.
		/// @throws ParseErrorException if template cannot be parsed due
		/// to syntax (or other) error.
		/// @throws Exception if a problem in parse
		///
		/// </returns>
		public Resource GetResource(string resourceName, ResourceType resourceType, string encoding)
		{
			/*
			* Check to see if the resource was placed in the cache.
			* If it was placed in the cache then we will use
			* the cached version of the resource. If not we
			* will load it.
			*/

			Resource resource = globalCache.get(resourceName);

			if (resource == null)
			{
				try
				{
					// it's not in the cache, so load it.
					resource = LoadResource(resourceName, resourceType, encoding);

					if (resource.ResourceLoader.CachingOn)
					{
						globalCache.put(resourceName, resource);
					}
				}
				catch (ResourceNotFoundException)
				{
					runtimeServices.Error(
						string.Format("ResourceManager : unable to find resource '{0}' in any resource loader.", resourceName));

					throw;
				}
				catch (ParseErrorException pee)
				{
					runtimeServices.Error(string.Format("ResourceManager.GetResource() parse exception: {0}", pee));

					throw;
				}
				catch (Exception ee)
				{
					runtimeServices.Error(string.Format("ResourceManager.GetResource() exception new: {0}", ee));

					throw;
				}
			}
			else
			{
				/*
					*  refresh the resource
					*/

				try
				{
					RefreshResource(resource, encoding);
				}
				catch (ResourceNotFoundException)
				{
					/*
						*  something exceptional happened to that resource
						*  this could be on purpose, 
						*  so clear the cache and try again
						*/

					globalCache.remove(resourceName);

					return GetResource(resourceName, resourceType, encoding);
				}
				catch (ParseErrorException parseErrorException)
				{
					runtimeServices.Error(string.Format("ResourceManager.GetResource() exception: {0}", parseErrorException));

					throw;
				}
				catch (Exception exception)
				{
					runtimeServices.Error(string.Format("ResourceManager.GetResource() exception: {0}", exception));

					throw;
				}
			}

			return resource;
		}

		/// <summary>
		/// Loads a resource from the current set of resource loaders
		/// </summary>
		/// <param name="resourceName">The name of the resource to retrieve.</param>
		/// <param name="resourceType">The type of resource (<code>Template</code>,
		/// <code>Content</code>, etc.).
		/// </param>
		/// <param name="encoding"> The character encoding to use.</param>
		/// <returns>Resource with the template parsed and ready.
		/// @throws ResourceNotFoundException if template not found
		/// from any available source.
		/// @throws ParseErrorException if template cannot be parsed due
		/// to syntax (or other) error.
		/// @throws Exception if a problem in parse
		/// </returns>
		protected internal Resource LoadResource(string resourceName, ResourceType resourceType, string encoding)
		{
			Resource resource = ResourceFactory.GetResource(resourceName, resourceType);

			resource.RuntimeServices = runtimeServices;

			resource.Name = resourceName;
			resource.Encoding = encoding;

			/*
			* Now we have to try to find the appropriate
			* loader for this resource. We have to cycle through
			* the list of available resource loaders and see
			* which one gives us a stream that we can use to
			* make a resource with.
			*/

			long howOldItWas = 0; // Initialize to avoid warnings

			ResourceLoader resourceLoader = null;

			for (int i = 0; i < resourceLoaders.Count; i++)
			{
				resourceLoader = (ResourceLoader)resourceLoaders[i];
				resource.ResourceLoader = resourceLoader;

				/*
		*  catch the ResourceNotFound exception
		*  as that is ok in our new multi-loader environment
		*/

				try
				{
					if (resource.Process())
					{
						/*
			*  FIXME  (gmj)
			*  moved in here - technically still 
			*  a problem - but the resource needs to be 
			*  processed before the loader can figure 
			*  it out due to to the new 
			*  multi-path support - will revisit and fix
			*/

						if (logWhenFound)
						{
							runtimeServices.Info(
								string.Format("ResourceManager : found {0} with loader {1}", resourceName, resourceLoader.ClassName));
						}

						howOldItWas = resourceLoader.GetLastModified(resource);
						break;
					}
				}
				catch (ResourceNotFoundException)
				{
					/*
					*  that's ok - it's possible to fail in
					*  multi-loader environment
					*/
				}
			}

			/*
			* Return null if we can't find a resource.
			*/
			if (resource.Data == null)
			{
				throw new ResourceNotFoundException(string.Format("Unable to find resource '{0}'", resourceName));
			}

			/*
			*  some final cleanup
			*/

			resource.LastModified = howOldItWas;

			resource.ModificationCheckInterval = resourceLoader.ModificationCheckInterval;

			resource.Touch();

			return resource;
		}

		/// <summary>  Takes an existing resource, and 'refreshes' it.  This
		/// generally means that the source of the resource is checked
		/// for changes according to some cache/check algorithm
		/// and if the resource changed, then the resource data is
		/// reloaded and re-parsed.
		/// *
		/// </summary>
		/// <param name="resource">resource to refresh
		/// *
		/// @throws ResourceNotFoundException if template not found
		/// from current source for this Resource
		/// @throws ParseErrorException if template cannot be parsed due
		/// to syntax (or other) error.
		/// @throws Exception if a problem in parse
		///
		/// </param>
		/// <param name="encoding"></param>
		protected internal void RefreshResource(Resource resource, string encoding)
		{
			/*
			* The resource knows whether it needs to be checked
			* or not, and the resource's loader can check to
			* see if the source has been modified. If both
			* these conditions are true then we must reload
			* the input stream and parse it to make a new
			* AST for the resource.
			*/
			if (resource.RequiresChecking())
			{
				/*
		*  touch() the resource to reset the counters
		*/

				resource.Touch();

				if (resource.IsSourceModified())
				{
					/*
				*  now check encoding info.  It's possible that the newly declared
				*  encoding is different than the encoding already in the resource
				*  this strikes me as bad...
				*/

					if (!resource.Encoding.Equals(encoding))
					{
						runtimeServices.Error(
							string.Format("Declared encoding for template '{0}' is different on reload.  Old = '{1}'  New = '{2}",
														resource.Name, resource.Encoding, encoding));

						resource.Encoding = encoding;
					}

					/*
				*  read how old the resource is _before_
				*  processing (=>reading) it
				*/
					long howOldItWas = resource.ResourceLoader.GetLastModified(resource);

					/*
				*  read in the fresh stream and parse
				*/

					resource.Process();

					/*
				*  now set the modification info and reset
				*  the modification check counters
				*/

					resource.LastModified = howOldItWas;
				}
			}
		}

		/// <summary> Gets the named resource.  Returned class type corresponds to specified type
		/// (i.e. <code>Template</code> to <code>Template</code>).
		/// *
		/// </summary>
		/// <param name="resourceName">The name of the resource to retrieve.
		/// </param>
		/// <param name="resourceType">The type of resource (<code>Template</code>,
		/// <code>Content</code>, etc.).
		/// </param>
		/// <returns>Resource with the template parsed and ready.
		/// @throws ResourceNotFoundException if template not found
		/// from any available source.
		/// @throws ParseErrorException if template cannot be parsed due
		/// to syntax (or other) error.
		/// @throws Exception if a problem in parse
		/// *
		/// </returns>
		/// <deprecated>Use
		/// {@link #GetResource(string resourceName, int resourceType,
		/// string encoding )}
		///
		/// </deprecated>
		public Resource GetResource(string resourceName, ResourceType resourceType)
		{
			return GetResource(resourceName, resourceType, RuntimeConstants.ENCODING_DEFAULT);
		}

		/// <summary>  Determines is a template exists, and returns name of the loader that
		/// provides it.  This is a slightly less hokey way to support
		/// the Velocity.templateExists() utility method, which was broken
		/// when per-template encoding was introduced.  We can revisit this.
		/// </summary>
		/// <param name="resourceName">Name of template or content resource
		/// </param>
		/// <returns>class name of loader than can provide it
		///
		/// </returns>
		public string GetLoaderNameForResource(string resourceName)
		{
			ResourceLoader resourceLoader;

			/*
			*  loop through our loaders...
			*/
			for (int i = 0; i < resourceLoaders.Count; i++)
			{
				resourceLoader = (ResourceLoader)resourceLoaders[i];

				Stream input = null;

				// if we find one that can provide the resource,
				// return the name of the loaders's Class
				try
				{
					input = resourceLoader.GetResourceStream(resourceName);

					if (input != null)
					{
						return resourceLoader.GetType().ToString();
					}
				}
				catch (ResourceNotFoundException)
				{
					// this isn't a problem.  keep going
				}
				finally
				{
					// if we did find one, clean up because we were 
					// returned an open stream
					if (input != null)
					{
						try
						{
							input.Close();
						}
						catch (IOException)
						{
						}
					}
				}
			}

			return null;
		}
	}
}