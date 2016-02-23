﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez.Tiled
{
	public class TiledMap
	{
		#region Fields and Properties

		public int firstGid;
		public int width;
		public int height;
		public int tileWidth;
		public int tileHeight;
		public Color? backgroundColor;
		public TiledRenderOrder renderOrder;
		public TiledMapOrientation orientation;
		public Dictionary<string,string> properties = new Dictionary<string,string>();
		public List<TiledLayer> layers = new List<TiledLayer>();
		public List<TiledImageLayer> imageLayers = new List<TiledImageLayer>();
		public List<TiledTileLayer> tileLayers = new List<TiledTileLayer>();
		public List<TiledObjectGroup> objectGroups = new List<TiledObjectGroup>();

		public int widthInPixels
		{
			get { return width * tileWidth - width; }       
		}

		public int heightInPixels
		{
			get { return height * tileHeight - height; }
		}

		readonly List<TiledTileset> _tilesets = new List<TiledTileset>();
		internal List<TiledAnimatedTile> _animatedTiles = new List<TiledAnimatedTile>();

		#endregion


		public TiledMap(
			int firstGid,
			int width, 
			int height, 
			int tileWidth, 
			int tileHeight, 
			TiledMapOrientation orientation = TiledMapOrientation.Orthogonal )
		{
			this.firstGid = firstGid;
			this.width = width;
			this.height = height;
			this.tileWidth = tileWidth;
			this.tileHeight = tileHeight;
			this.orientation = orientation;
		}


		public TiledTileset createTileset( Texture2D texture, int firstId, int tileWidth, int tileHeight, int spacing = 2, int margin = 2 )
		{
			var tileset = new TiledTileset( texture, firstId, tileWidth, tileHeight, spacing, margin );
			_tilesets.Add( tileset );
			return tileset;
		}


		public TiledTileLayer createTileLayer( string name, int width, int height, TiledTile[] tiles )
		{
			var layer = new TiledTileLayer( this, name, width, height, tiles );
			layers.Add( layer );
			return layer;
		}


		public TiledImageLayer createImageLayer( string name, Texture2D texture, Vector2 position )
		{
			var layer = new TiledImageLayer( name, texture, position );
			layers.Add( layer );
			return layer;
		}


		public TiledObjectGroup createObjectGroup( string name, Color color, bool visible, float opacity )
		{
			var group = new TiledObjectGroup( name, color, visible, opacity );
			objectGroups.Add( group );
			return group;
		}


		public TiledLayer getLayer( string name )
		{
			for( var i = 0; i < layers.Count; i++ )
			{
				if( layers[i].name == name )
					return layers[i];
			}
			return null;
		}


		public T getLayer<T>( string name ) where T : TiledLayer
		{
			return (T)getLayer( name );
		}


		/// <summary>
		/// handles calling update on all animated tiles
		/// </summary>
		public void update()
		{
			for( var i = 0; i < _animatedTiles.Count; i++ )
				_animatedTiles[i].update();
		}


		/// <summary>
		/// calls draw on each visible layer
		/// </summary>
		/// <param name="spriteBatch">Sprite batch.</param>
		/// <param name="position">Position.</param>
		/// <param name="layerDepth">Layer depth.</param>
		/// <param name="cameraClipBounds">Camera clip bounds.</param>
		public void draw( SpriteBatch spriteBatch, Vector2 position, float layerDepth, RectangleF cameraClipBounds )
		{
			// render any visible image or tile layer
			foreach( var layer in layers )
			{
				if( !layer.visible )
					continue;

				layer.draw( spriteBatch, position, layerDepth, cameraClipBounds );
			}
		}


		public void drawWithoutCullOrPositioning( SpriteBatch spriteBatch, bool useMapBackgroundColor = false )
		{
			if( useMapBackgroundColor && backgroundColor.HasValue )
				spriteBatch.GraphicsDevice.Clear( backgroundColor.Value );

			foreach( var layer in layers )
				layer.draw( spriteBatch );
		}


		/// <summary>
		/// gets the TiledTileset for the given tileId
		/// </summary>
		/// <returns>The tileset for tile identifier.</returns>
		/// <param name="id">Identifier.</param>
		public TiledTileset getTilesetForTileId( int tileId )
		{
			for( var i = _tilesets.Count - 1; i >= 0; i-- )
			{
				if( _tilesets[i].firstId <= tileId )
					return _tilesets[i];
			}

			throw new Exception( string.Format( "tileId {0} was not foind in any tileset", tileId ) );
		}


		/// <summary>
		/// returns the TiledTilesetTile for the given id or null if none exists. TiledTilesetTiles exist only for animated tiles.
		/// </summary>
		/// <returns>The tileset tile.</returns>
		/// <param name="id">Identifier.</param>
		public TiledTilesetTile getTilesetTile( int id )
		{
			for( var i = _tilesets.Count - 1; i >= 0; i-- )
			{
				if( _tilesets[i].firstId <= id )
				{
					for( var j = 0; j < _tilesets[i].tiles.Count; j++ )
					{
						// id is a gid so we need to subtract the tileset.firstId to get a local id
						if( _tilesets[i].tiles[j].id == id - _tilesets[i].firstId )
							return _tilesets[i].tiles[j];
					}
				}
			}

			return null;
		}


		public int worldPositionToTilePositionX( float x )
		{
			var tileX = (int)Math.Floor( x / tileWidth );
			return Mathf.clamp( tileX, 0, width - 1 );
		}


		public int worldPositionToTilePositionY( float y )
		{
			var tileY = (int)Math.Floor( y / tileHeight );
			return Mathf.clamp( tileY, 0, height - 1 );
		}
	
	}
}
