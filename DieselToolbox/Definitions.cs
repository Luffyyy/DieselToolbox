using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;

namespace DieselToolbox
{
    public static class Definitions
    {
        public static string[] ScriptDataExtensions = 
        {
            "sequence_manager",
            "environment",
            "menu",
            "continent",
            "continents",
            "mission",
            "nav_data",
            "cover_data",
            "world",
            "world_cameras",
            "prefhud",
            "objective",
            "credits",
            "hint",
            "comment",
            "dialog",
            "dialog_index",
            "timeline",
            "action_message",
            "achievment",
            "controller_settings",
            "world_sounds"
        };

        public static string[] RawTextExtension = 
        {
            "unit",
            "material_config",
            "object",
            "animation_def",
            "animation_states",
            "animation_state_machine",
            "animation_subset",
            "merged_font",
            "physic_effect",
            "post_processor",
            "scene",
            "gui",
            "effect",
            "render_template_database",
            "xml",
            "network_settings",
            "xbox_live",
            "atom_batcher_settings",
            "camera_shakes",
            "cameras",
            "decals",
            "physics_settings",
            "scenes",
            "texture_channels",
            "diesel_layers",
            "light_intensities"
        };

		public static Dictionary<string, Icon> FolderIcon = new Dictionary<string, Icon>{
			{"open", Icon.FromResource("DieselToolbox.ImageResources.folder-open.ico")},
			{"closed", Icon.FromResource("DieselToolbox.ImageResources.folder.ico")}
		};

		public static Dictionary<string, Icon> FileIcons = new Dictionary<string, Icon>{
			{"movie",  Icon.FromResource("DieselToolbox.ImageResources.film.ico")},
			{"texture",  Icon.FromResource("DieselToolbox.ImageResources.image.ico")},
			{"font",  Icon.FromResource("DieselToolbox.ImageResources.font.ico")},
			{"text",  Icon.FromResource("DieselToolbox.ImageResources.document-text.ico")},
			{"strings",  Icon.FromResource("DieselToolbox.ImageResources.document-text.ico")},
			{"default",  Icon.FromResource("DieselToolbox.ImageResources.document.ico")},
        };

		public static string TypeFromExtension(string ext)
		{
			if (RawTextExtension.Contains (ext))
				return "text";
			else if (ScriptDataExtensions.Contains (ext))
				return "scriptdata";

			return ext;
		}

        public static double MinimumHorizontalDragDistance { get => 8; }
        public static double MinimumVerticalDragDistance { get => 8; }

        public static string TempDir = "Browser";

        public static string HashDir = "./Hashlists";
        public static string AddedHashDir = "Extra";

        public static string FolderTypeName = "File Folder";
    }
}
