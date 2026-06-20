import { Office365UsersService } from "@/generated"
import type { User } from "@/generated/models/Office365UsersModel";
import type { IOperationResult } from "@microsoft/power-apps/data";
import { useEffect, useState } from "react"

export default function MyProfilePage() {

    const [profile, setProfile] = useState<IOperationResult<User>|null>(null)

    useEffect(() => {
        let ignore = false; // To track if the component is still mounted
        Office365UsersService.MyProfile().then(data => {
            console.log("Received profile data:", data)
            if (!ignore) {
                console.log("Fetched profile data:", data)
                setProfile(data)
            }
        }).catch(error => {
            if (!ignore) {
                console.error("Error fetching profile:", error)
            }
        })

        return () => {
            ignore = true; // Mark as unmounted
            // Cleanup if necessary when the component unmounts
        }
    }, [])

  return (
    <div>My Profile JSON: {JSON.stringify(profile?.data)}</div>
  )
}